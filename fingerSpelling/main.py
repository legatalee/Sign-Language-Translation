import cv2
import mediapipe
import numpy
import pandas
from collections import deque
from PIL import ImageFont, ImageDraw, Image
import time
import pyautogui
import pickle
import platform
import sys

import xgboost as xgb


model_path = "Model/xgb_model46.json"
label_path = "./labels.csv"
keyboard_path = "./keyboard.p"


with open(keyboard_path, 'rb') as f:
    KEYBOARD_DICT = pickle.load(f)

FONT_SIZE = 200 # 글자 표시 사이즈
COLOR = (255, 0, 255) # 글자 색깔

SPEED_LIMIT = 0.05 # 손 끝 속도 기준치
TIME_FRAME = 0.2 # 속도 계산 시간차

mp_hands = mediapipe.solutions.hands  # hand model
mp_drawing = mediapipe.solutions.drawing_utils  # frawing utils


def mediapipe_detection(image, model):
    image = cv2.cvtColor(image, cv2.COLOR_BGR2RGB)  # color conversion
    image.flags.writeable = False  # image is no longer writeable
    results = model.process(image)  # detection
    image.flags.writeable = True  # image is writeable
    image = cv2.cvtColor(image, cv2.COLOR_RGB2BGR)
    return image, results 



# OpenCV 이미지에 한글 그려주는 함수
def myPutText(src, text, pos, font_size, font_color):
    img_pil = Image.fromarray(src)
    draw = ImageDraw.Draw(img_pil)
    font_path = 'fonts/gulim.ttc'
    if platform.system() == "Darwin":
        font_path = 'AppleGothic.ttf'

    font = ImageFont.truetype(font_path, font_size)  # 윈도우 에서는 'fonts/gulim.ttc', 애플 "AppleGothic.ttf"
    draw.text(pos, text, font=font, fill=font_color)
    return numpy.array(img_pil)


# 머신러닝 detection 함수
def detect_ML(input_array):
    pred = model.predict(xgb.DMatrix(input_array.reshape(1, -1)))[0]
    pred = numpy.argmax(pred)
    return labels["val"][pred]

# 타이핑 함수
def keyboard(cur_res_final):
    if cur_res_final == 'ㅚ':
        pyautogui.write(KEYBOARD_DICT['ㅗ'])
        pyautogui.write(KEYBOARD_DICT['ㅣ'])
    elif cur_res_final == 'ㅟ':
        pyautogui.write(KEYBOARD_DICT['ㅜ'])
        pyautogui.write(KEYBOARD_DICT['ㅣ'])
    elif cur_res_final == 'ㅢ':
        pyautogui.write(KEYBOARD_DICT['ㅡ'])
        pyautogui.write(KEYBOARD_DICT['ㅣ'])
    else:
        pyautogui.write(KEYBOARD_DICT[cur_res_final])

cap = cv2.VideoCapture(0)
landmark_input = deque()

cur_res_final = ""
prev_res_final = ""

labels = pandas.read_csv(label_path, index_col=0)
double_const = {"ㄱ": "ㄲ", "ㄷ": "ㄸ", "ㅂ": "ㅃ", "ㅅ": "ㅆ", "ㅈ": "ㅉ"}

final_type_store = []

cap.set(cv2.CAP_PROP_FRAME_WIDTH, 1280)
cap.set(cv2.CAP_PROP_FRAME_HEIGHT, 720)

WIDTH = cap.get(cv2.CAP_PROP_FRAME_WIDTH)
HEIGHT = cap.get(cv2.CAP_PROP_FRAME_HEIGHT)
print('해상도 : {} X {}'.format(WIDTH, HEIGHT))


model = xgb.Booster()
model.load_model(model_path)


prev_hand_end = numpy.full((5, 3), 0.5)  # 손가락 끝 좌표를 저장할 변수
prev_time = time.time()  # 속도계산을 위한 시간 저장 변수

last_input_time = time.time()
input_debounce_duration = 1

while True:
    ret, frame = cap.read()
    if not ret:
        print("카메라 인식 불가")
        break

    with mp_hands.Hands(
        min_detection_confidence=0.5, min_tracking_confidence=0.5, max_num_hands=2
    ) as hands:
        image, results = mediapipe_detection(frame, hands)
        if results.multi_hand_landmarks:
            for idx, hand_landmarks in enumerate(results.multi_hand_landmarks):
                mp_drawing.draw_landmarks(
                    image, hand_landmarks, mp_hands.HAND_CONNECTIONS)

                hand_array = numpy.array(
                    [[res.x, res.y, res.z] for res in hand_landmarks.landmark])
                cur_hand_end = hand_array[[4, 8, 12, 16, 20], :]
                hand_array = hand_array[:, :2].flatten()

                dist = numpy.sqrt(numpy.sum((cur_hand_end - prev_hand_end)**2, axis=1))
                cur_time = time.time()
                if cur_time - prev_time > TIME_FRAME:
                    speed = dist / TIME_FRAME
                    if any(speed < SPEED_LIMIT):
                        cur_res_final = detect_ML(hand_array)
                        if(cur_res_final.isdigit()) : cur_res_final = ""
                    prev_time = time.time()
                prev_hand_end = cur_hand_end
        else: cur_res_final = ""

        cur_res_final_list = cur_res_final.split(",")
        cur_res_final = cur_res_final_list[-1]
        # Validate and use result
        if cur_res_final and cur_res_final != prev_res_final and (time.time() - last_input_time > input_debounce_duration):
            keyboard(cur_res_final)
            final_type_store.append(cur_res_final)
            prev_res_final = cur_res_final
            print(cur_res_final, " ", sep="", end="")
            sys.stdout.flush()
            last_input_time = time.time()
            

        image = cv2.flip(image, 1)
        image = myPutText(image, cur_res_final, (570, 30), FONT_SIZE, COLOR)

        cv2.imshow("frame", image)
        if cv2.waitKey(10) & 0xFF == 13:
            print("\n입력 :", final_type_store)
            break

cap.release()
cv2.destroyAllWindows()