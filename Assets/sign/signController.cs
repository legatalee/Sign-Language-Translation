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
    Animator anim;
    private static BlockingCollection<string> messageQueue = new BlockingCollection<string>();
    ThreadSafeBoolean isAnimating = new ThreadSafeBoolean(false);

    Dictionary<KeyCode, string> mapAlphabetical = new Dictionary<KeyCode, string>
    {
        { KeyCode.R, "giyeok" },
        { KeyCode.S, "nieun" },
        { KeyCode.E, "digeut" },
        { KeyCode.F, "rieul" },
        { KeyCode.A, "mieum" },
        { KeyCode.Q, "bieup" },
        { KeyCode.T, "shiot" },
        { KeyCode.D, "ieung" },
        { KeyCode.W, "jieut" },
        { KeyCode.C, "chieuch" },
        { KeyCode.Z, "kieuk" },
        { KeyCode.X, "tieut" },
        { KeyCode.V, "pieup" },
        { KeyCode.G, "hieu" },
        { KeyCode.K, "a" },
        { KeyCode.I, "ya" },
        { KeyCode.J, "eo" },
        { KeyCode.U, "yeo" },
        { KeyCode.H, "o" },
        { KeyCode.Y, "yo" },
        { KeyCode.N, "u" },
        { KeyCode.B, "yu" },
        { KeyCode.M, "eu" },
        { KeyCode.L, "i" },
        { KeyCode.O, "ae" },
        { KeyCode.P, "e" },
    };

    Dictionary<string, string> mapPhrase = new Dictionary<string, string>
    {
        { "안녕", "hello" },
        { "나", "me" },
        { "저", "me" },
        { "이름", "name" },
        { "이다", "is" },
    };

    // Start is called before the first frame update
    void Start()
    {
        anim = gameObject.GetComponent<Animator>();
        SocketThread myJob = new SocketThread();
        myJob.Schedule();
    }

    // Update is called once per frame
    void Update()
    {
        if (!isAnimating.Value)
        {
            foreach (var (key, action) in mapAlphabetical)
            {
                if (Input.GetKeyDown(key))
                {
                    isAnimating.Value = true;
                    StartCoroutine(PlayAnimationAndWait(action));
                }
            }
            if (messageQueue.Count > 0)
            {
                string action = messageQueue.Take();
                if (mapPhrase.ContainsKey(action))
                {
                    isAnimating.Value = true;
                    StartCoroutine(PlayAnimationAndWait(mapPhrase[action]));
                }
            }
        }
    }

    private IEnumerator PlayAnimationAndWait(string animName)
    {
        // 애니메이션을 크로스페이드로 실행
        anim.CrossFade(animName, 0.2f);
        yield return new WaitForSeconds(0.2f);

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

    struct SocketThread : IJob
    {
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
                    Console.WriteLine("Server started...");

                    // 연결을 계속해서 받아들임
                    while (true)
                    {
                        Console.WriteLine("Waiting for a connection...");

                        // 클라이언트 연결을 비동기적으로 대기
                        TcpClient client = server.AcceptTcpClient();
                        Console.WriteLine("Connected!");

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
                                Console.WriteLine("Received: {0}", data);

                                // 메시지를 큐에 추가
                                messageQueue.Add(data);

                                // 응답 전송
                                byte[] msg = Encoding.UTF8.GetBytes(data);
                                stream.Write(msg, 0, msg.Length);
                                Console.WriteLine("Sent: {0}", data);
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
}
