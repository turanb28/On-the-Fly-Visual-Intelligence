# On the Fly Visual Intelligence

![Project Demo](https://private-user-images.githubusercontent.com/98922140/417639137-a3be601a-e976-4e38-807c-a024b6909dd7.png?jwt=eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJpc3MiOiJnaXRodWIuY29tIiwiYXVkIjoicmF3LmdpdGh1YnVzZXJjb250ZW50LmNvbSIsImtleSI6ImtleTUiLCJleHAiOjE3Njg2ODEyODYsIm5iZiI6MTc2ODY4MDk4NiwicGF0aCI6Ii85ODkyMjE0MC80MTc2MzkxMzctYTNiZTYwMWEtZTk3Ni00ZTM4LTgwN2MtYTAyNGI2OTA5ZGQ3LnBuZz9YLUFtei1BbGdvcml0aG09QVdTNC1ITUFDLVNIQTI1NiZYLUFtei1DcmVkZW50aWFsPUFLSUFWQ09EWUxTQTUzUFFLNFpBJTJGMjAyNjAxMTclMkZ1cy1lYXN0LTElMkZzMyUyRmF3czRfcmVxdWVzdCZYLUFtei1EYXRlPTIwMjYwMTE3VDIwMTYyNlomWC1BbXotRXhwaXJlcz0zMDAmWC1BbXotU2lnbmF0dXJlPTU0N2RjYWYxODA5YTliYmU3Njg2MGQwNzFiNjM1Y2UzYjRmM2QzYjU2ZTBiYWY2OTYzMDhlZWNkZDEwMTExMDgmWC1BbXotU2lnbmVkSGVhZGVycz1ob3N0In0.ViDgToW_QO3PfiQ9W6uT3GlxXuQXhJfIEtM1_swB2Es)

> **Real-time computer vision powered by YOLO and .NET.**

**On the Fly Visual Intelligence** is a high-performance C# application designed to execute YOLO (You Only Look Once) models for real-time visual analysis. Whether you are processing static images, video files, or live streams, this tool provides immediate inference results for object detection and segmentation tasks.

## 🚀 Features

*   **Real-Time Processing**: optimized for low-latency inference on video streams.
*   **Multiple Input Sources**: 
    *   📂 **Images**: Analyze single or batch images.
    *   🎥 **Videos**: Process pre-recorded video files.
    *   📡 **Live Streams**: Support for webcams and RTSP/HTTP streams.
*   **Model Support**: Currently supports standard Object Detection and Instance Segmentation.
*   **C# Native**: Built entirely in C# for seamless integration with the .NET ecosystem.

## 🗺️ Roadmap

We are actively developing this project. Upcoming features include:

- [ ] **Oriented Bounding Box (OBB)**: Detection for rotated objects.
- [ ] **Pose Estimation**: Skeleton keypoint detection for human pose analysis.
- [ ] **Classification**: Whole-image image classification support.
- [ ] **Model Export/Import**: Easier swapping of custom-trained YOLO models.

## 🛠️ Prerequisites

Before you begin, ensure you have the following installed:

*   [.NET SDK](https://dotnet.microsoft.com/download) (Version 8.0 or later recommended)
*   [Visual Studio 2022](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/)

## 📥 Installation

1.  **Clone the repository:**
    ```bash
    git clone https://github.com/turanb28/On-the-Fly-Visual-Intelligence.git
    cd On-the-Fly-Visual-Intelligence
    ```

2.  **Open the project:**
    Open `OnTheFly_UI.sln` in Visual Studio.

3.  **Restore Dependencies:**
    Visual Studio should automatically restore the necessary NuGet packages. If not, run:
    ```bash
    dotnet restore
    ```

## ▶️ Usage

1.  **Build the Solution:**
    Press `Ctrl + Shift + B` or navigate to **Build > Build Solution**.

2.  **Run the Application:**
    Press `F5` to start the application with debugging.

3.  **Select Source & Model:**
    *   Use the UI to select your input source (Camera, Video File, or Image).
    *   Load your desired YOLO model weights (e.g., `yolov8n.pt` or ONNX format if applicable).
    *   Click **Start** to begin inference.

## 🤝 Contributing

Contributions are welcome! This project is in its early stages, and we appreciate help with new features, bug fixes, and documentation.

1.  Fork the repository.
2.  Create your feature branch (`git checkout -b feature/AmazingFeature`).
3.  Commit your changes (`git commit -m 'Add some AmazingFeature'`).
4.  Push to the branch (`git push origin feature/AmazingFeature`).
5.  Open a Pull Request.

## 📄 License

Distributed under the MIT License. See `LICENSE` for more information. (If a license file is not yet present, please refer to the repository owner).

---

*Stay tuned for updates!* 🚀