# WSIST — Simplified Auto-Deploy with Keel

Replace the Tailscale + SSH deploy approach with Keel, a lightweight
image-polling tool that runs on the cluster and automatically rolls out
new images when they're pushed to ghcr.io.

## How this works

1. GitHub Actions builds and pushes the image to ghcr.io (already working)
2. Keel runs on the cluster, watches ghcr.io for new image tags
3. When a new `latest` tag is pushed, Keel triggers a rollout automatically
4. No SSH, no Tailscale, no kubectl from GitHub Actions needed

## Workflow rules

- One branch: `feat/keel-deploy`
- Run `dotnet test` and `dotnet csharpier .` before committing
- CodeRabbit review required before merge

---

## Task 1 — Simplify the GitHub Actions deploy workflow

File: `.github/workflows/deploy.yaml`

Replace the entire file with this simplified version that only builds and
pushes the image. Remove all Tailscale, SSH, and kubectl steps entirely.

```yaml
name: Deploy to k3s

on:
  push:
    branches: [ main ]

jobs:
  build-and-push:
    runs-on: ubuntu-latest

    permissions:
      contents: read
      packages: write

    steps:
      - name: Checkout
        uses: actions/checkout@v4

      - name: Set image tag
        id: tag
        run: echo "sha=${GITHUB_SHA::8}" >> $GITHUB_OUTPUT

      - name: Log in to ghcr.io
        uses: docker/login-action@v3
        with:
          registry: ghcr.io
          username: ${{ github.actor }}
          password: ${{ secrets.GITHUB_TOKEN }}

      - name: Build and push image
        uses: docker/build-push-action@v6
        with:
          context: .
          push: true
          tags: |
            ghcr.io/timh8127/wsist:${{ steps.tag.outputs.sha }}
            ghcr.io/timh8127/wsist:latest
```

---

## Task 2 — Add Keel manifest

Create `k8s/keel.yaml`. Keel runs in its own namespace and needs permission
to update Deployments and StatefulSets across the cluster.

```yaml
apiVersion: v1
kind: Namespace
metadata:
  name: keel
---
apiVersion: v1
kind: ServiceAccount
metadata:
  name: keel
  namespace: keel
---
apiVersion: rbac.authorization.k8s.io/v1
kind: ClusterRole
metadata:
  name: keel
rules:
  - apiGroups: [""]
    resources: ["namespaces"]
    verbs: ["watch", "list"]
  - apiGroups: ["", "extensions", "apps"]
    resources: ["pods", "replicationcontrollers", "replicasets",
                "deployments", "daemonsets", "statefulsets"]
    verbs: ["get", "list", "watch", "update", "patch"]
  - apiGroups: [""]
    resources: ["configmaps", "secrets"]
    verbs: ["get", "list", "watch"]
  - apiGroups: ["batch"]
    resources: ["jobs"]
    verbs: ["get", "list", "watch"]
---
apiVersion: rbac.authorization.k8s.io/v1
kind: ClusterRoleBinding
metadata:
  name: keel
roleRef:
  apiGroup: rbac.authorization.k8s.io
  kind: ClusterRole
  name: keel
subjects:
  - kind: ServiceAccount
    name: keel
    namespace: keel
---
apiVersion: apps/v1
kind: Deployment
metadata:
  name: keel
  namespace: keel
  labels:
    app: keel
spec:
  replicas: 1
  selector:
    matchLabels:
      app: keel
  template:
    metadata:
      labels:
        app: keel
    spec:
      serviceAccountName: keel
      containers:
        - name: keel
          image: keelhq/keel:latest
          imagePullPolicy: Always
          env:
            - name: NAMESPACE
              value: ""
          ports:
            - containerPort: 9300
          livenessProbe:
            httpGet:
              path: /healthz
              port: 9300
            initialDelaySeconds: 30
            periodSeconds: 10
          readinessProbe:
            httpGet:
              path: /healthz
              port: 9300
            initialDelaySeconds: 10
            periodSeconds: 5
```

---

## Task 3 — Add Keel annotation to wsist-app Deployment

File: `k8s/app.yaml`

Add the following annotation to the Deployment's `spec.template.metadata`
section. This tells Keel to watch the `latest` tag and roll out
automatically when it changes:

```yaml
spec:
  template:
    metadata:
      labels:
        app: wsist-app
      annotations:
        keel.sh/policy: force
        keel.sh/trigger: poll
        keel.sh/pollSchedule: "@every 1m"
```

`force` policy means Keel will update even when the tag (`latest`) doesn't
change — it compares the image digest instead. Poll interval of 1 minute
means new deploys are live within 60 seconds of the image push completing.

---

## Commit messages

- `ci: simplify deploy workflow — build and push only, Keel handles rollout`
- `feat(k8s): add Keel for automatic image-based deployment`
- `feat(k8s): add Keel poll annotation to wsist-app deployment`
