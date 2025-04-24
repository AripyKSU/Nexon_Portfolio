using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Threading.Tasks;
using Samples.Whisper;

public class ConversationManager : MonoBehaviour
{
    public static ConversationManager conversationManager;

    //지금 대화중인 지 판별하는 변수
    private bool m_isTalking = false;
    public bool IsTalking
    {
        get { return m_isTalking; }
        set
        {
            m_isTalking = value;
            Debug.Log("Is Talking: " + m_isTalking);
        }
    }

    //누구의 대화 턴인지 판별하는 변수
    private bool m_talkingTurn = false; // true: player's turn, false: NPC's turn
    public bool TalkingTurn
    {
        get { return m_talkingTurn; }
        set { m_talkingTurn = value; }
    }

    private string dialogues = "";
    private StringBuilder stringBuilder = new StringBuilder();

    //서로의 대화에 필요한 내용을 담는 변수 
    private string playerText = null;
    private bool playerInput = false;
    private string npcText = null;
    private bool npcInput = false;

    //대화가 끝나는 상황인지 판단하는 변수
    private bool justOVERFlag = false;

    //대화 coroutine을 담는 변수
    private Coroutine conversation = null;

    public _ChatGPTManager _ChatGPTManager;
    public TTS _TTS;
    public STT _STT;

    //플레이어가 말한것을 대화에 추가함
    public void PlayerSay(string text)
    {
        playerText = text;
        playerInput = true;
        AppendDialogue("User: ", playerText);
    }

    public void NPCSay(string text)
    {
        npcText = text;

        bool stopFlag = false;

        //파인튜닝을 통해 대화가 끝날만한 상황에 gpt가 !OVER을 붙여서 답을 보내면 호출되어 대화를 끝냄
        if (text.StartsWith("!OVER"))
        {
            stopFlag = true;
            justOVERFlag = true;
            npcText = npcText.Replace("!OVER", "");
        }
        else if (text.Contains("!OVER"))
        {
            stopFlag = true;
            npcText = npcText.Replace("!OVER", "");
        }

        AppendDialogue("Assistant: ", npcText);

        //npcText를 _TTS로 재생
        _TTS._TTS_Play(npcText);

        if (!justOVERFlag)
        {
            //애니메이션 실행
            if (NpcType == "NPC: Cafe Clerk")
            {
                CafeClerk.GetComponent<Animator>().SetTrigger("talk");
            }
            //friend
            else if (NpcType == "NPC: Old Friend")
            {
                OldFriend.GetComponent<Animator>().SetTrigger("talk");
            }
            //mugcup
            else if (NpcType == "NPC: Mug Cup")
            {
                CafeClerk.GetComponent<Animator>().SetTrigger("talk");
            }
            //bump
            else if (NpcType == "NPC: Bump")
            {
                Bump.GetComponent<Animator>().SetTrigger("talk");
            }
        }
        npcInput = true;

        if (stopFlag)
        {
            StopConversation();
        }

    }

    public async void StartConversation()
    {
        Debug.Log("StartConversation");
        //이미 다른 대화를 하고 있을 때 이벤트 발생 시 무시
        if (IsTalking != false)
        {
            return;
        }
        IsTalking = true;
        _ChatGPTManager.SetEval(false);
        await playerConversationControl.FadeOut();

        //npc layer에 따라 다르게
        if (NpcType == "NPC: Cafe Clerk")
        {
            _ChatGPTManager.AskChatGPT("You are a helpful assistant who is a cafe clerk. ");
        }
        //friend
        else if (NpcType == "NPC: Old Friend")
        {
            _ChatGPTManager.AskChatGPT("You are a helpful assistant, who meet your old friend, Sarah");
        }
        //mugcup
        else if (NpcType == "NPC: Mug Cup")
        {
            _ChatGPTManager.AskChatGPT("You are a helpful assistant who is a clerk who sells a mugcup");
        }
        //bump
        else if (NpcType == "NPC: Bump")
        {
            _ChatGPTManager.AskChatGPT("You are a helpful assistant, and a stranger who bumped a shoulder with me. ");
        }
        TalkingTurn = true;

        //_STT 키는 거
        _STT.StartRecording();

        //Conversation 시작
        await playerConversationControl.FadeIn();

        conversation = StartCoroutine(Conversation());

    }

    public async void StopConversation()
    {
        //Conversation 코루틴 중지 및 각종 변수 초기화
        StopCoroutine(conversation);
        conversation = null;
        IsTalking = false;
        _TTS.AudioStop();

        await Task.Delay(1000);
        await playerConversationControl.FadeOut();

        //_STT(마이크) 끄는 거
        _STT.StopRecording();

        //대화 종료 후 대화 내용 평가
        EvaluateDialogue();

        //다음 대화를 위해 gptmanager의 대화 내용 초기화
        FlushTexts();
        _ChatGPTManager.ClearMessage();

        await playerConversationControl.FadeIn();
        TalkingTurn = false;
    }

    public IEnumerator Conversation()
    {
        //서로 반복되며 대화 진행
        while (IsTalking)
        {
            //player turn, 플레이어의 마이크로 인풋이 오기 전까지 대기
            _STT.StartRecording();
            yield return new WaitUntil(() => playerInput);
            TalkingTurn = false;
            playerInput = false;
            _ChatGPTManager.AskChatGPT(playerText);

            //npc turn, gpt에게 응답이 오기 전까지 대기
            yield return new WaitUntil(() => npcInput);
            TalkingTurn = true;
            npcInput = false;            
        }
    }

    //stringBuilder를 사용하여 대화 내용을 붙여가며 저장
    public void AppendDialogue(string subject, string text)
    {
        stringBuilder.Append(dialogues);
        stringBuilder.Append(subject + text + "\n");
        dialogues = stringBuilder.ToString();
        stringBuilder = new StringBuilder();
    }

    //대화 내용 비우기
    public void FlushTexts()
    {
        dialogues = "";
    }

    //gptmanager에 대화 내용 평가 요청
    public void EvaluateDialogue()
    {
        _ChatGPTManager.SetEval(true);
        _ChatGPTManager.ClearMessage();
        string prompt = "You are an English language tutor specializing in conversational skills. I will provide you with dialogue scripts between user and assistant. Your task is to review these scripts and provide detailed feedback on any awkward or unnatural parts of user's dialogue. Don't worry about case letters. Specifically, identify:\n\n1. Grammatical errors\n2. Unnatural phrasing or vocabulary usage\n\nFor each identified issue, please explain why it is problematic.\n\nAnd your answers should be like this:\n[1. Grammatical errors:\n\"I good.\" is grammatically wrong. \"I\'m good.\" is grammatically right.\n2. Unnatural phrasing:\nWhile \"I\'m dialing emergency services now.\" is correct, \"I\'m calling emergency services now.\" is more common term.\n]\n\nHere is the dialogue script for your review:";
        _ChatGPTManager.AskChatGPT(prompt + dialogues);
    }
}
