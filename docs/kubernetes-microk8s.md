# MicroK8s no Windows – Instalação e Configuração do Kubectl

## MicroK8s

MicroK8s é uma distribuição leve e certificada do Kubernetes, criada pela Canonical, ideal para desenvolvimento, testes, ambientes locais e edge computing. Ele oferece um cluster Kubernetes completo, com suporte a addons como DNS, ingress, dashboard, storage e muito mais, utilizando baixo consumo de recursos.

[Siga as instruções oficiais para instalação no Windows](https://microk8s.io/#install-microk8s)

---

## Instalar o kubectl no Windows

O kubectl é a ferramenta de linha de comando usada para interagir com clusters Kubernetes.

[Siga as instruções oficiais para instalação no Windows](https://kubernetes.io/docs/tasks/tools/install-kubectl-windows/)

---

## Exportar a configuração do MicroK8s para o kubectl

Após instalar o MicroK8s e garantir que o cluster está funcionando, execute no terminal onde o MicroK8s está disponível:

```powershell
cd $HOME\.kube
microk8s config > config
```

Esse comando cria o arquivo `config`, que permite ao `kubectl` se conectar ao cluster MicroK8s.

---

## Validar a conexão com o cluster

Após exportar a configuração, teste se o kubectl está conectado corretamente:

```powershell
kubectl get nodes
```

Se estiver funcionando, você verá o node **microk8s-vm**  listado com o status **Ready**.

## Habilitar hostpath-storage addon no MicroK8s

O addon `hostpath-storage` permite o uso de volumes persistentes no MicroK8s. Para habilitá-lo, execute:
```powershell
microk8s enable hostpath-storage
```

