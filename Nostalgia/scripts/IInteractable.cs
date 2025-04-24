namespace Nostal.Interfaces
{
    public interface IInteractable
    {
        //Input System의 E키를 눌렀을 때 호출되는 함수
        void OnInteract(Fusion.NetworkObject playerObject);
    }
}
