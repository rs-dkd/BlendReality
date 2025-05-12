# BlenderVR
 
<p align="center">
  <br>
  <em>A Virtual Reality Interface for 3D Modeling</em>
</p>

## 🌟 Overview

BlenderVR is an immersive virtual reality application that brings professional 3D modeling capabilities into VR. Inspired by Blender's powerful feature set, BlenderVR allows direct manipulation of 3D objects in virtual space, making complex modeling operations more intuitive while maintaining the precision and capability of desktop modeling software.

### 🎯 Project Objectives

- Create a fully-functional VR modeling application with Blender-like capabilities
- Implement advanced modeling features (Boolean operations, NURBS, knife tools, etc.)
- Design an intuitive, VR-optimized user interface
- Ensure cross-platform compatibility across major VR systems
- Fill the gap in professional-grade modeling tools available in VR

## ✨ Features

### Features

- [ ] VR Movement and Navigation
- [ ] Hand Tracking and Controller Input
- [ ] Tool Selection System
- [ ] 3D Menu Framework
- [ ] Primitive Shape Creation
- [ ] Vertex/Edge/Face Selection
- [ ] Transform Tools (Move/Rotate/Scale)
- [ ] Boolean Operations (Union, Difference, Intersection)
- [ ] NURBS Curve and Surface Support
- [ ] Knife and Cutting Tools
- [ ] UV Unwrapping
- [ ] Color and Material Selection
- [ ] Import/Export of Standard 3D Formats
- [ ] Direct Blender Integration

## 🛠️ Technology Stack

- **Game Engine**: Unity 2022.3 LTS+
- **Programming**: C#
- **VR SDKs**: 
  - Unity XR Interaction Toolkit
  - Oculus Integration Plugin
- **3D Modeling Reference**: Blender
- **Version Control**: Git/GitHub
- **Testing Platforms**: Oculus Quest 2, additional VR headsets as available

## 📋 Prerequisites

- Unity 2022.3 LTS or newer
- Oculus Integration from the Unity Asset Store
- Git/GitHub
- VR headset for testing (primary: Oculus Quest 2)

## 🚀 Getting Started

### Installation

1. Clone the repository with Git:
   ```bash
   git clone https://github.com/YourUsername/BlenderVR.git

Open the project in Unity:

Open Unity Hub
Click "Add" and browse to the cloned project directory
Open the project with Unity 2022.3 LTS or newer


Install required packages:

In Unity, go to Window > Package Manager
Install XR Plugin Management
Install XR Interaction Toolkit
Import Oculus Integration from the Asset Store


Configure for VR development:

Go to Edit > Project Settings > XR Plugin Management
Enable for your target platform (Oculus, etc.)



### Project Structure

- **/BlenderVR**
  - **/Assets/**
    - **/Scripts/** - C# scripts organized by feature
    - **/Prefabs/** - Reusable VR components
    - **/Materials/** - Materials and shaders
    - **/Scenes/** - Unity scenes for different features
    - **/Plugins/** - Third-party packages
    - **/Samples/** - Example and demonstration content
    - **/Settings/** - Project configuration files
    - **/VR/** - VR-specific components and systems
    - **/XR/** - XR framework components
  - **/Packages/** - Unity package dependencies
  - **/ProjectSettings/** - Unity project configuration
  - **/Documentation/** - Project documentation
  - **/Tools/** - Helper tools for development
  - **/Tests/** - Testing scripts and procedures
  - **.gitignore** - Git ignore rules
  - **README.md** - Project documentation

### Development Workflow

1. **Branch Strategy**:
   - `main`: Production-ready code
   - `developer`: Integration branch
   - Feature branches: `feature/feature-name`
   - Bug fixes: `fix/issue-number-description`

2. **Contributing**:
   - Create a feature branch from `developer`
   - Implement your changes
   - Submit a pull request to `developer`
   - Request review from team members

## 📊 Project Roadmap

| Milestone | Description | Timeline |
|-----------|-------------|----------|
| Setup & Environment | Project setup, basic VR environment | May 7 - May 14 |
| Core UI Framework | Tool selection, 3D menus, settings | May 14 - May 25 |
| Import/Export System | File format support, asset pipeline | May 25 - June 1 |
| Basic Modeling | Primitives, selection, transforms | June 1 - June 15 |
| Advanced Modeling | Boolean ops, NURBS, knife tools | June 15- June 29 |
| Texturing & Materials | UV unwrapping, materials, colors | June 29 - July 13 |
| Testing & Completion | Optimization, testing, documentation | July 13 - July 27 |

## 🧪 Testing

- **Unit Testing:** Test coverage for core modeling operations
- **Integration Testing:** Testing feature interactions
- **User Testing:** Feedback from professional 3D modelers and VR users
- **Performance Testing:** Frame rate optimization and memory usage

## 📚 Documentation

- **User Guide:** End-user documentation
- **Developer Guide:** Technical documentation
- **API Reference:** Scripting interfaces
- **Feature Specifications:** Detailed feature descriptions

## 🤝 Contributions

Code Standards

Follow Unity C# coding conventions
Comment all public methods and properties
Write unit tests for new features
Update documentation for any changes

## 👥 Team

### [Developers]
- Reggie Segovia (Project Lead)
- Michael Termotto
- Guanyu Chen

## 📞 Contact
For questions or feedback about BlenderVR, please open an issue on this repository or contact me at [rdavidsegovia@gmail.com/reggie.segovia@ufl.edu].
