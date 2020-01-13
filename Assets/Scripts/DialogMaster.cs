﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine.SceneManagement;
using UnityEngine;

public class DialogMaster : MonoBehaviour
{
    GameObject master;
    ChoiceControl choiceControl;

    System.Net.Sockets.NetworkStream ns;
    System.Net.Sockets.TcpClient tcp;

    // <**シーンの初期設定
    public static string[] Scenes = { "ST", "TE" };
    public static int SceneNum = 0; // どのシーンにいるのか
    public static int SequenceTENum = 1; // 今シーンTEでどの遷移状態にいるのか
    // シーンの初期設定**>

    // Start is called before the first frame update
    void Start()
    {
        // <**まずresources以下にあるcsvファイルをすべて読み込む
        ConstantsDic.commUNetworkSettings = ConstantsDic.ReadCSV("network_setting");

        // 発話の読み込み
        ConstantsDic.mainClaims = ConstantsDic.ReadCSV("main_claims");
        ConstantsDic.SequenceTE = ConstantsDic.ReadCSV("001_te_sequence");
        ConstantsDic.TranScriptST = ConstantsDic.ReadCSV("000_st");
        ConstantsDic.TranScriptTE = ConstantsDic.ReadCSV("001_te");
        ConstantsDic.OnScreenTE = ConstantsDic.ReadCSV("001_te_onScreen");

        // まずresources以下にあるcsvファイルをすべて読み込む**>


        master = GameObject.Find("Master");
        choiceControl = master.GetComponent<ChoiceControl>();


        // <** サーバーに接続
        string ipOrHost = "127.0.0.1";
        int port = 1000;
        for (int i = 0; i < 6; i++)
        {
            if (ConstantsDic.commUNetworkSettings[i][0] == Names.ID)
            {
                ipOrHost = ConstantsDic.commUNetworkSettings[i][1];
                port = int.Parse(ConstantsDic.commUNetworkSettings[i][2]);
            }
        }

        if (port == 0)
        {
            SceneManager.LoadScene("InPutName");
            // 間違ったIDを入れたらもう一度入れ直し
        }
        else
        {
            //TcpClientを作成し、サーバーと接続
            try
            {
                // <**テスト用！後で消します
                ipOrHost = "127.0.0.1";
                port = 1000;
                // テスト用！後で消します**>
                tcp = new System.Net.Sockets.TcpClient(ipOrHost, port);
                Debug.Log("サーバー({0}:{1})と接続しました。" +
                    ((System.Net.IPEndPoint)tcp.Client.RemoteEndPoint).Address + "," +
                    ((System.Net.IPEndPoint)tcp.Client.RemoteEndPoint).Port + "," +
                    ((System.Net.IPEndPoint)tcp.Client.LocalEndPoint).Address + "," +
                    ((System.Net.IPEndPoint)tcp.Client.LocalEndPoint).Port);

                //NetworkStreamを取得
                ns = tcp.GetStream();

                //接続出来たらロボットが初めの挨拶をする
                Debug.Log("初期化:" + Scenes[SceneNum] + SequenceTENum.ToString("D2"));
                string[] temp;
                temp = ConstantsDic.SearchUtterance(Names.ID, Scenes[SceneNum], 1, ConstantsDic.TranScriptST);
                MessageSender(ConstantsDic.FixTranscript(temp[3], Names.ID));

                SceneNum++;


            }
            catch (SocketException e)
            {
                Debug.Log("接続に失敗しました");
                SceneManager.LoadScene("InPutName");
            }

        }
        // サーバーに接続**>


    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            ns.Close();
            tcp.Close();
            Debug.Log("切断しました。");
        }
    }

    public void MessageSender(string data)
    {
        System.Text.Encoding enc = System.Text.Encoding.UTF8;
        byte[] sendBytes = enc.GetBytes(data + '\n');
        //データを送信する
        ns.Write(sendBytes, 0, sendBytes.Length);
        Debug.Log(data);
    }

    public void OnButtonProceed()
    {

    }

    public void OnButtonConfirm()
    {
        // 進むボタンを押したときにシーンを更新する、文章を描画しなおす、選択肢ボタンを配置しなおす
        Debug.Log("Now we are:" + Scenes[SceneNum] + SequenceTENum.ToString("D2"));
        choiceControl.PlaceChoice(Scenes[SceneNum], SequenceTENum);
    }

    public void OnButtonFix()
    {
        // 確認ウィンドウで、修正をすることにした場合
        SceneDrawer.IsInConfirmation = false;
    }
}
