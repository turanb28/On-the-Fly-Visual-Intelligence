# 👁️ On-the-Fly Visual Intelligence

![GitHub License](https://img.shields.io/github/license/turanb28/On-the-Fly-Visual-Intelligence)
![.NET Version](https://img.shields.io/badge/.NET-8.0-purple)
![Platform](https://img.shields.io/badge/Platform-Windows-blue)
![Status](https://img.shields.io/badge/Status-Active%20Development-green)

> **Real-time, high-performance visual analysis powered by YOLO and .NET.**

**On the Fly Visual Intelligence** is a robust C# application designed for seamless object detection, segmentation, and visual data analysis. Built on the latest .NET ecosystem, it allows users to perform inference on static images, video files, and live camera feeds with low latency.

---

## 📸 Demo

<img width="1439" height="997" alt="Review" src="https://github.com/user-attachments/assets/2a7a8608-dfaa-48e9-bdfe-50f795a2c69c" />

---

## ✨ What's New?

The project has undergone significant updates to improve performance and usability:
- **Expanded Model Support:** Now supports **YOLOv8 / YOLOv9 / YOLOv10** models.
- **Advanced Tasks:** Added support for **Oriented Bounding Boxes (OBB)** and **Pose Estimation**.
- **UI Overhaul:** A completely redesigned user interface for easier model swapping and parameter tuning.
- **GPU Acceleration:** Enhanced CUDA support for faster real-time inference.

---

## 🚀 Features

### 🔍 Core Capabilities
*   **Real-Time Inference:** Optimized for high FPS on live video streams (Webcam / RTSP).
*   **Multi-Tasking:** Switch between Object Detection, Instance Segmentation, and Pose Estimation on the fly.
*   **Universal Input:** Drag-and-drop support for images and videos, plus auto-detection for connected cameras.

### 🛠️ Advanced Tools
*   **Model Hot-Swapping:** Load custom `.onnx` models instantly without restarting the app.
*   **Adjustable Thresholds:** Fine-tune **Confidence** and **IoU (Intersection over Union)** sliders in real-time to filter noise.
*   **Visual Analytics:** View bounding boxes, class labels, and confidence scores overlaid directly on the feed.

---

## 💻 Tech Stack

*   **Language:** C# (.NET 8.0)
*   **Framework:** WPF 
*   **Computer Vision:** OpenCVSharp, YoloDotNet (or Ultralytics wrappers)
*   **Acceleration:** CUDA (NVIDIA GPU support recommended)

---

## 🛠️ Installation & Setup

### Prerequisites
1.  **[.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)** or higher.
2.  **Visual Studio 2022** (Community/Professional).
3.  *(Optional for GPU)* **NVIDIA CUDA Toolkit** and **cuDNN** compatible with your GPU drivers.

### Steps
1.  **Clone the Repository**
    ```bash
    git clone https://github.com/turanb28/On-the-Fly-Visual-Intelligence.git
    cd On-the-Fly-Visual-Intelligence
    ```

2.  **Open in Visual Studio**
    Double-click the `OnTheFly_UI.sln` file.

3.  **Restore Dependencies**
    Visual Studio should handle this automatically. If not, run:
    ```bash
    dotnet restore
    ```

4.  **Download Models**
    Ensure you have your YOLO weights (e.g., `yolov8n.onnx`, `yolov8s-seg.onnx`) ready.

5.  **Build & Run**
    Press `F5` or click **Start** in Visual Studio.

---

## 🎮 Usage Guide

1.  **Select Input Source:**
    *   **Camera:** Choose your webcam from the dropdown list.
    *   **Media:** Click "Open File" to load a video (`.mp4`) or image.
2.  **Load Model:**
    *   Click "Load Model" and select your `.onnx` file.
    *   Select the task type (Detection, Segmentation, Pose) if not auto-detected.
3.  **Tune Parameters:**
    *   Adjust the **Confidence Threshold** slider (default: 0.50).
4.  **Start Inference:**
    *   Click the **Start/Stop** button to toggle processing.

---

## 📄 License

Distributed under the MIT License.
