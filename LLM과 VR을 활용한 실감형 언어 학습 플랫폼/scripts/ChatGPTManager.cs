using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using OpenAI;

public class ChatGPTManager : MonoBehaviour
{
    private OpenAIApi openAI = new OpenAIApi(
        organization: //
        apiKey: //
    );
    private List<ChatMessage> messages = new List<ChatMessage>();
    private bool evalMode = false;

    //question을 받아서 ChatGPT에게 물어보는 함수
    public async void AskChatGPT(string question)
    {
        ChatMessage newMessage = new ChatMessage();
        newMessage.Content = question;
        newMessage.Role = "user";

        messages.Add(newMessage);

        //OpenAI API에서 ChatMessage를 여러개 받아 멀티턴 대화를 하게 하는 클래스
        CreateChatCompletionRequest request = new CreateChatCompletionRequest();
        
        request.Messages = messages;
        //평가 모드일 경우 gpt-4o 모델을, 대화 모드일 경우 파인튜닝한 모델을 사용
        if(!evalMode)
            request.Model = "ft:gpt-3.5-turbo-0125:miff:conversation-final://";
        else
            request.Model = "gpt-4o";

        var response = await openAI.CreateChatCompletion(request);

        //대답으로 오는 것중 첫째번을 선택하여 messages에 추가
        if(response.Choices != null && response.Choices.Count > 0)
        {
            var chatResponse = response.Choices[0].Message;
            messages.Add(chatResponse);

            //대화 모드일 경우 생성된 응답으로 대화가 이어지도록 함 
            if(!evalMode)
                ConversationManager.conversationManager.NPCSay(chatResponse.Content);
        }
    }

    public void ClearMessage() {
        messages.Clear();
    }

    //eval 변수를 바꿔서 평가중인지 아닌지 확인
    public void SetEval(bool mode) {
        Debug.Log("Evalmode is changed: " + mode);
        evalMode = mode;
    }

}
