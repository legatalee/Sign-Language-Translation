import speech_recognition as sr
import asyncio
from time import localtime
import socket


recording_time = 10  # second
process_count = 5

lock = asyncio.Lock()
sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
try:
    sock.settimeout(5)
    sock.connect(('127.0.0.1', 6346))
except:
    pass


async def record_and_recognize() -> None:
    recognizer = sr.Recognizer()
    recognizer.non_speaking_duration = 0.1
    recognizer.pause_threshold = 0.15
    with sr.Microphone() as source:
        while True:
            await lock.acquire()
            l = localtime()
            audio = recognizer.listen(source, phrase_time_limit=10)
            lock.release()
            text = ''
            try:
                text = recognizer.recognize_google(audio, language='ko-KR')
            except:
                pass
            print(f'{l.tm_hour}h:{l.tm_min}m:{l.tm_sec}s: {text}')


async def main() -> None:
    print("Started.")
    await asyncio.gather(*[record_and_recognize() for _ in range(process_count)])
    print("Ended.")


asyncio.run(main())
