Unity-based AR interface for **EEG/BCI-driven target selection** and **robot grasping**.  
The app receives **brain signals via OSC**, lets the user **select and confirm objects in AR**, and then sends the **target ID to a robot controller** (e.g., Raspberry Pi + MyCobot) via HTTP.

基于 Unity 的 **脑机接口 + 增强现实 + 机械臂抓取** 项目：通过脑电指令在手机 AR 画面中选择/确认目标物体，并将目标编号发送给机械臂端执行抓取任务。

---

## 1. Project Overview

This project is part of a larger **BCI–AR–Robot** closed-loop system:

1. **BCI / EEG**  
   - Emotiv (or other) EEG headset  
   - Motor imagery commands (e.g., `/left`, `/right`, `/lift`, `/confirm`) encoded as OSC messages

2. **AR-BCI-Unity (this repo)**  
   - Runs on an Android phone  
   - Uses AR (marker / QR / image tracking) to recognize multiple physical objects  
   - Uses EEG commands to move a “selection cursor” between objects and confirm the final target  
   - Highlights state:  
     - **Idle** – all targets white  
     - **Selected** – current target turns yellow and scales up  
     - **Confirmed** – target turns green, and its ID is sent to the robot

3. **Robot side (e.g., Raspberry Pi + MyCobot)**  
   - Receives the target ID (A/B/C/…) from the phone via **HTTP** (and/or OSC)  
   - Maps ID → visual marker ID (e.g., STAG / ArUco)  
   - Runs camera-based pose estimation + hand–eye calibration  
   - Executes the corresponding **grasping motion**

> In short: **脑电信号 → 手机 AR 目标选择 → 发送目标编号 → 机械臂视觉识别并抓取**。

---

## 2. Main Features

- **EEG-driven target selection**
  - Use BCI commands to switch between multiple AR targets
  - Final confirmation via a dedicated EEG command (e.g., `/confirm` or `/lift`)

- **AR-based multi-object scene**
  - Plane detection + marker / image tracking
  - Each object is visualized as a 3D arrow / marker
  - Dynamic color + scale to indicate selection state

- **Communication bridge**
  - **OSC** input from BCI → Unity (e.g., from BCI-OSC / Python scripts)
  - **HTTP POST** (or OSC) output from Unity → robot controller
  - Message format e.g.:
    ```json
    { "target": "A" }
    ```

- **Test utilities**
  - `TestFile/test_confirm.py` for sending test `/confirm` signals to Unity when BCI is not available

---

## 3. Repository Structure

At the top level:

```text
AR-BCI-Unity/
├─ Assets/           # Unity assets: scenes, prefabs, scripts, materials, etc.
├─ Packages/         # Unity packages (AR Foundation, etc.)
├─ ProjectSettings/  # Unity project configuration
├─ TestFile/
│  └─ test_confirm.py  # Python script to send OSC 'confirm' signal for testing
├─ .gitignore
└─ .vsconfig
````

> Note: The detailed scripts and scenes are under `Assets/`. Key components include AR setup, OSC receiver, and HTTP client for robot communication.

---

## 4. Requirements

### Unity & AR

* Unity (2020+; recommended an LTS version)
* AR support:

  * AR Foundation / ARCore XR Plugin (for Android)
  * Properly configured AR scene (camera, session origin, etc.)
* Mobile device:

  * Android phone (e.g., Xiaomi 14) with ARCore support

### BCI / OSC side

* EEG device (e.g., Emotiv EPOC series)
* Software to send OSC:

  * EmotivBCI + BCI-OSC (or any custom Python script)
  * Network reachable from the phone (same Wi-Fi / hotspot)

### Robot side (optional but recommended)

* Raspberry Pi (robot controller)
* Robot arm (e.g., **MyCobot 280Pi** or similar)
* Python 3.x environment
* Flask (or similar HTTP server) to receive target ID, e.g.:

  * `POST /target` with JSON `{ "target": "A" }`
* Vision + calibration code (STAG / ArUco marker detection, hand–eye calibration, etc.)

---

## 5. Getting Started

### 5.1 Clone & Open in Unity

```bash
git clone https://github.com/Junzhe/AR-BCI-Unity.git
cd AR-BCI-Unity
```

1. Open the project in Unity Hub.
2. Let Unity import all assets and packages.
3. Open the main AR scene under `Assets/` (e.g., your AR demo scene).

### 5.2 Configure OSC Input (BCI → Unity)

1. In Unity, locate the script / GameObject that receives OSC (e.g., a receiver component).
2. Set:

   * **Listening IP**: usually `0.0.0.0` on the phone
   * **Port**: must match the port used by your BCI OSC sender
3. Configure the expected OSC addresses, for example:

   * `/left`, `/right` — switch selected target
   * `/lift` or `/confirm` — confirm the current target

#### Testing with `test_confirm.py`

1. Ensure the phone and PC are on the **same network**.
2. In `TestFile/test_confirm.py`, set:

   * `UNITY_IP` → phone IP
   * `UNITY_PORT` → OSC port Unity listens on
3. Run:

   ```bash
   python test_confirm.py
   ```
4. You should see the AR target change to a “confirmed” state in Unity.

### 5.3 Configure HTTP Output (Unity → Robot)

1. In Unity, find the script that sends the target ID to the robot (e.g., HTTP client).

2. Set:

   * Robot controller URL, e.g.: `http://<raspberry_pi_ip>:5000/target`

3. Confirm that the Raspberry Pi is running a Flask server like:

   ```python
   @app.route("/target", methods=["POST"])
   def target():
       data = request.get_json()
       target_code = data.get("target", "A")
       # Map target_code -> STAG/ArUco ID and start grasping
       ...
   ```

4. On EEG confirmation:

   * Unity picks the current target (A/B/C…)
   * Sends HTTP POST to `/target`
   * Robot executes visual recognition + grasping pipeline

---

## 6. Interaction Logic

**Default visual logic (可按需修改):**

1. **Initialization**

   * All AR targets are spawned as small **white** arrows

2. **Selection (EEG /left, /right, …)**

   * Active target turns **yellow**
   * Scale increases slightly to emphasize focus

3. **Confirmation (EEG /confirm or /lift)**

   * Target turns **green**
   * Unity continuously or once-off sends the target ID to the robot controller
   * Robot starts grasping procedure

This interface is designed for **zero-touch, multimodal interaction**, combining **EEG intent** + **AR visualization** + **robot actions**.

---

## 7. Roadmap / TODO

* [ ] Add screenshots / demo GIFs of the AR interface
* [ ] Provide example Unity scenes and prefabs for quick start
* [ ] Release sample Python code for:

  * BCI OSC sender
  * Flask robot controller + MyCobot grasping
* [ ] Link to associated paper / preprint once published

---

## 8. Acknowledgements

This repository is developed as part of an ongoing research project on **multimodal BCI–AR–Robot collaboration**.
If you use or extend this project for academic research, please consider acknowledging:

> *Junzhe Wang, et al. “EEG-Driven AR–Robot Grasping System for Zero-Touch Manipulation” (work in progress).*

---

## 9. Contact

For questions, issues, or collaboration:

* **Author**: Junzhe Wang
* **GitHub**: [@Junzhe](https://github.com/Junzhe)

欢迎提 issue 或者直接联系作者，一起交流脑机接口 + AR + 机器人方向的研究与开发 🙌
