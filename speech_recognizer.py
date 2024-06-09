import speech_recognition as sr
from time import localtime
import socket
from threading import Thread, Lock
from konlpy.tag import Okt


sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
try:
    sock.settimeout(5)
    sock.connect(('127.0.0.1', 6346))
    print("connected")
except:
    print("not connected")


class RecognizerThread(Thread):
    record_lock = Lock()
    print_lock = Lock()

    def __init__(self, source: sr.Microphone) -> None:
        super().__init__(daemon=True)
        self.source = source
        self.recognizer = sr.Recognizer()
        self.recognizer.non_speaking_duration = 0.1
        self.recognizer.pause_threshold = 0.15
        self.okt = Okt()

    def run(self):
        while True:
            self.record_lock.acquire()
            l = localtime()
            audio = self.recognizer.listen(source, phrase_time_limit=10)
            t = localtime()
            self.record_lock.release()
            text = ''
            try:
                text = self.recognizer.recognize_google(audio, language='ko-KR')
            except:
                pass
            pharsed = self.okt.morphs(text, stem=True)
            for stem in pharsed:
                sock.send(stem.encode("utf-8"))
                sock.recv(4096)
            self.print_lock.acquire()
            print(f'{"%2d" % l.tm_hour}h:{"%2d" % l.tm_min}m:{"%2d" % l.tm_sec}s ~ {"%2d" % t.tm_hour}h:{"%2d" % t.tm_min}m:{"%2d" % t.tm_sec}s | {text}')
            print(" " * 25 + " | " + str(pharsed))
            self.print_lock.release()


if __name__ == "__main__":
    with sr.Microphone() as source:
        threads = [RecognizerThread(source) for _ in range(5)]
        for thread in threads:
            thread.start()
        print("Started")
        okt = Okt()
        while True:
            stems = okt.morphs(input(), stem=True)
            print(stems)
            for stem in stems:
                sock.send(stem.encode("utf-8"))
                sock.recv(4096)
