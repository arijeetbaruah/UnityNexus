<p align="center">
  <img src="docs/images/social-preview.png" alt="Maths Engine Banner"/>
</p>

![CodeRabbit Pull Request Reviews](https://img.shields.io/coderabbit/prs/github/arijeetbaruah/Maths-Engine?utm_source=oss&utm_medium=github&utm_campaign=arijeetbaruah%2FMaths-Engine&labelColor=171717&color=FF570A&link=https%3A%2F%2Fcoderabbit.ai&label=CodeRabbit+Reviews)

<h1 align="center">Maths Engine</h1>

<p align="center">
Serializable Math Formula System for Unity
</p>

<p align="center">

<img src="https://img.shields.io/github/stars/arijeetbaruah/Maths-Engine?style=for-the-badge">
<img src="https://img.shields.io/github/license/arijeetbaruah/Maths-Engine?style=for-the-badge">
<img src="https://img.shields.io/github/issues/arijeetbaruah/Maths-Engine?style=for-the-badge">

</p>

---

## ✨ Overview

**Maths Engine** is a modular system for building **serializable mathematical formulas in Unity**.

Instead of hardcoding equations in scripts, you can create flexible formula graphs using reusable **Math Nodes**.

This makes it ideal for systems like:

* Damage calculations
* AI decision formulas
* Gameplay balancing
* Procedural systems
* Conditional logic

---

## 🚀 Features

* Modular **node-based math system**
* Fully **serializable formulas**
* Runtime **formula evaluation**
* Human-readable **equation generation**
* Extensible **custom math nodes**
* Logical operators for conditional formulas
* Comparison operators

---

## 🧩 Example Formula

Example conditional formula:

```text
(HP < 30 AND EnemyDistance < 5) ? Damage * 2 : Damage
```

Generated equation output:

```text
((HP < 30 AND EnemyDistance < 5) ? (Damage * 2) : Damage)
```

---

## 📦 Installation

### Unity Package Manager (Git URL)

1. Open **Unity Package Manager**
2. Click **Add package from git URL**
3. Paste:

```
https://github.com/arijeetbaruah/Maths-Engine.git?path=Packages/com.arijeet.mathsengine/MathsEngine
```

Unity will install the package automatically.

---

### Manual Installation

Clone the repository:

```
git clone https://github.com/arijeetbaruah/Maths-Engine.git
```

Copy the runtime folder into your Unity project:

```
Assets/
 └── MathsEngine/
```

Unity will compile the scripts automatically.

---

### Verify Installation

1. Right-click in the Project window
2. Navigate to:

```
Create → Baruah → Maths Engine → Maths Formula
```

3. Create a **Math Formula asset**

If the asset appears, installation succeeded.

---

## 📖 Documentation

Full documentation is available here:

https://arijeetbaruah.github.io/Maths-Engine/

Getting started guide:

https://arijeetbaruah.github.io/Maths-Engine/getting_started.html

---

## 🏗 Architecture

Maths Engine is built around modular **Math Nodes**.

```
BaseMathNode
 ├── Arithmetic Nodes
 ├── Comparison Nodes
 ├── Logical Nodes
 └── Custom Nodes
```

Each node can evaluate itself and generate a readable equation.

---

## 🤝 Contributing

Contributions are welcome!

Please read:

```
CONTRIBUTING.md
```

before submitting a pull request.

Pull requests should follow the repository template.

---

## 🌟 Contributors

Thanks to all contributors who help improve this project.

<a href="https://github.com/arijeetbaruah/Maths-Engine/graphs/contributors">
  <img src="https://contrib.rocks/image?repo=arijeetbaruah/Maths-Engine" />
</a>

---

## 🛡 License

This project is licensed under the **MIT License**.

---

## ⭐ Support the Project

If you find this project useful:

* ⭐ Star the repository
* 🐛 Report issues
* 🧩 Contribute new nodes
* 📢 Share it with other Unity developers
