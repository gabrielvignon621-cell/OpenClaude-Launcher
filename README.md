<div align="center">

# 🟢 OpenClaude Launcher

**Abra o OpenClaude com um clique, num terminal bonito e já configurado.**

![Windows](https://img.shields.io/badge/Windows-10%2F11-0078D6?logo=windows&logoColor=white)
![C#](https://img.shields.io/badge/C%23-.NET%20Framework-512BD4?logo=csharp&logoColor=white)
![Node](https://img.shields.io/badge/Node.js-20%2B-339933?logo=node.js&logoColor=white)

</div>

---

## ✨ O que ele faz

- 🔍 **Localiza o OpenClaude** automaticamente (PATH ou `%APPDATA%\npm`)
- 🖥️ **Abre no Windows Terminal** com um perfil próprio: acrílico/blur, opacidade, padding e cursor em barra
- 🎨 **Tema "OpenClaude Soft"**: paleta escura suave e fonte JetBrainsMono Nerd Font (com fallbacks)
- 🧠 **Contexto de 1M tokens** definido sozinho via `/set-context-window`
- 🤖 **Modelo pré-selecionado**: `claude-opus-5-5`
- 📦 **Instalação assistida**: se o Windows Terminal não existir, oferece instalar via `winget` (junto com a fonte)
- 🪟 **Plano B**: sem Windows Terminal, abre no PowerShell clássico

## 📋 Requisitos

- Windows 10/11
- [Node.js 20+](https://nodejs.org)
- OpenClaude instalado globalmente:

```bash
npm install -g @twigpine/openclaude
```

## 🚀 Como usar

1. Compile o launcher:

```bash
csc /target:winexe /out:OpenClaude.exe /r:System.Windows.Forms.dll /r:Microsoft.VisualBasic.dll OpenClaude.cs
```

2. Dê dois cliques em `OpenClaude.exe`.
3. Pronto! O terminal abre com o banner, o modelo carregado e o contexto de 1M aplicado.

## ⚙️ Personalização

As constantes no topo do `OpenClaude.cs` controlam o essencial:

| Constante | Função |
|-----------|--------|
| `Model`   | Modelo usado ao iniciar |
| `CtxCmd`  | Comando de janela de contexto enviado após abrir |
| `Title`   | Título da janela |

O visual (cores, opacidade, fonte) fica no JSON do perfil dentro do código.

## ⚠️ Avisos

- O launcher inicia o OpenClaude com `--permission-mode bypassPermissions`, ou seja, **sem pedir confirmação** para ações. Use com cuidado, ou remova essa flag se preferir.
- O comando de contexto é enviado por simulação de teclado (`SendKeys`). Não mexa no teclado nos primeiros segundos após abrir.

## 📄 Licença

Distribuído sob a licença MIT. Veja `LICENSE` para mais detalhes.
