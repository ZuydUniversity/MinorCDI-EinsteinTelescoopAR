from flask import Flask, request, jsonify
import cv2
import mediapipe as mp
import numpy as np
import base64

app = Flask(__name__)
mp_hands = mp.solutions.hands.Hands(
    static_image_mode=False,
    max_num_hands=1,
    min_detection_confidence=0.5,
    min_tracking_confidence=0.5
)

@app.route("/handpose", methods=["POST"])
def handpose():
    try:
        data = request.json
        img_bytes = base64.b64decode(data["image"])
        nparr = np.frombuffer(img_bytes, np.uint8)
        img = cv2.imdecode(nparr, cv2.IMREAD_COLOR)

        # Convert BGR to RGB
        results = mp_hands.process(cv2.cvtColor(img, cv2.COLOR_BGR2RGB))

        landmarks = []
        if results.multi_hand_landmarks:
            for hand_landmarks in results.multi_hand_landmarks:
                landmarks.append([[lm.x, lm.y, lm.z] for lm in hand_landmarks.landmark])

        print(f"Sending landmarks: {landmarks}")
        return jsonify(landmarks)
    except Exception as e:
        return jsonify({"error": str(e)}), 500

if __name__ == "__main__":
    app.run(host="0.0.0.0", port=5000)
