using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections.Concurrent;
using System.Threading;
using Unity.Jobs;


class ThreadSafeBoolean
{
    private int _flag;

    public ThreadSafeBoolean(bool initialValue = false)
    {
        _flag = initialValue ? 1 : 0;
    }

    public bool Value
    {
        get => Interlocked.CompareExchange(ref _flag, 0, 0) == 1;
        set => Interlocked.Exchange(ref _flag, value ? 1 : 0);
    }
}


public class signController : MonoBehaviour
{
    private Animator anim;
    private static BlockingCollection<string> messageQueue = new BlockingCollection<string>();
    private ThreadSafeBoolean isAnimating = new ThreadSafeBoolean(false);

    Dictionary<KeyCode, string> mapAlphabetical = new Dictionary<KeyCode, string>
    {
        { KeyCode.R, "ㄱ" },
        { KeyCode.S, "ㄴ" },
        { KeyCode.E, "ㄷ" },
        { KeyCode.F, "ㄹ" },
        { KeyCode.A, "ㅁ" },
        { KeyCode.Q, "ㅂ" },
        { KeyCode.T, "ㅅ" },
        { KeyCode.D, "ㅇ" },
        { KeyCode.W, "ㅈ" },
        { KeyCode.C, "ㅊ" },
        { KeyCode.Z, "ㅋ" },
        { KeyCode.X, "ㅌ" },
        { KeyCode.V, "ㅍ" },
        { KeyCode.G, "ㅎ" },
        { KeyCode.K, "ㅏ" },
        { KeyCode.I, "ㅑ" },
        { KeyCode.J, "ㅓ" },
        { KeyCode.U, "ㅕ" },
        { KeyCode.H, "ㅗ" },
        { KeyCode.Y, "ㅛ" },
        { KeyCode.N, "ㅜ" },
        { KeyCode.B, "ㅠ" },
        { KeyCode.M, "ㅡ" },
        { KeyCode.L, "ㅣ" },
        { KeyCode.O, "ㅐ" },
        { KeyCode.P, "ㅔ" },
    };

    Dictionary<string, string> mapPhrase = new Dictionary<string, string>
    {
        { "안녕", "안녕" },
        { "안녕하다", "안녕" },
        { "만나다", "만나다" },
        { "반갑다", "즐겁다" },
        { "나", "나" },
        { "저", "나" },
        { "제", "나" },
        { "소개", "소개" },
        { "소개하다", "소개" },
        { "이름", "이름" },
        { "이다", "이다" },
        { "오늘", "오늘" },
        { "날씨", "날씨" },
        { "맑다", "맑다" },
        { "즐겁다", "즐겁다" },
        { "하루", "하루" },
        { "되다", "되다" },
    };

    // Start is called before the first frame update
    void Start()
    {
        anim = gameObject.GetComponent<Animator>();

        // 스레드 객체 생성
        Thread socketThread = new Thread(new ThreadStart(Execute));

        // 스레드를 데몬 스레드로 설정
        socketThread.IsBackground = true;

        // 스레드 시작
        socketThread.Start();
    }

    // Update is called once per frame
    void Update()
    {
        if (!isAnimating.Value)
        {
            /*
            foreach (var (key, action) in mapAlphabetical)
            {
                if (Input.GetKeyDown(key))
                {
                    isAnimating.Value = true;
                    StartCoroutine(PlayAnimationAndWait(action));
                    return;
                }
            }
            */
            if (messageQueue.Count > 0)
            {
                string action = messageQueue.Take();
                if (mapPhrase.ContainsKey(action))
                {
                    isAnimating.Value = true;
                    StartCoroutine(PlayAnimationAndWait(mapPhrase[action]));
                }
            }
            else
            {
                isAnimating.Value = true;
                StartCoroutine(PlayAnimationAndWait("idle"));
            }
        }
    }

    private IEnumerator PlayAnimationAndWait(string animName)
    {
        // 애니메이션을 크로스페이드로 실행
        anim.CrossFade(animName, 0.2f);
        yield return new WaitForSeconds(0.5f);

        // 현재 실행 중인 애니메이션 상태 정보를 가져옴
        AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);

        // 애니메이션이 끝날 때까지 대기
        while (stateInfo.IsName(animName) && stateInfo.normalizedTime < 1.0f)
        {
            yield return null; // 다음 프레임까지 대기
            stateInfo = anim.GetCurrentAnimatorStateInfo(0); // 상태 정보를 갱신
        }

        // 애니메이션이 끝난 후 실행할 코드
        isAnimating.Value = false;
    }


    public void Execute()
    {
        TcpListener server = null;
        while (true)
        {
            try
            {
                // 서버가 사용할 포트 번호
                Int32 port = 6346;
                // 로컬 주소
                IPAddress localAddr = IPAddress.Parse("127.0.0.1");

                // TcpListener 객체를 생성하여 서버 시작
                server = new TcpListener(localAddr, port);

                // 서버 시작
                server.Start();

                // 연결을 계속해서 받아들임
                while (true)
                {
                    // 클라이언트 연결을 비동기적으로 대기
                    TcpClient client = server.AcceptTcpClient();

                    // 클라이언트와 통신을 처리할 Task 생성
                    NetworkStream stream = client.GetStream();

                    int i;
                    byte[] bytes = new byte[1024];
                    string data;

                    try
                    {
                        // 클라이언트로부터 데이터 수신
                        while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                        {
                            data = Encoding.UTF8.GetString(bytes, 0, i);

                            // 메시지를 큐에 추가
                            messageQueue.Add(data);

                            // 응답 전송
                            byte[] msg = Encoding.UTF8.GetBytes(data);
                            stream.Write(msg, 0, msg.Length);
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Exception: {0}", e);
                    }
                    finally
                    {
                        // 스트림 및 클라이언트 소켓 닫기
                        stream.Close();
                        client.Close();
                    }
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }
            finally
            {
                // 서버를 중지
                server.Stop();
            }
        }
    }
}
