using UnityEngine;
using System;

public class ExploreEndSpot : MonoBehaviour, IInteractable
{
    public static event Action OnPlayerReturnToTown;   
    public string townSceneName = "Town";

    public string GetInteractPrompt() => "[E] 우주선 부르기";

    public void OnInteract()
    {
        // 팝업을 열면서 메시지와 할 일(람다식) 전달
        CommonConfirmPopup.Instance.OpenPopup(
            "탐사를 마치고 마을로 돌아가시겠습니까?",
            () => {
                Debug.Log("마을로 이동 중...");
                OnPlayerReturnToTown?.Invoke();
            }
        );
    }

}


//==========//=============================
//public class BedInteractable// Mo//Behaviour, IInteractable
//{
//    publi//CommonConfirmPopup popupUI;

///    pu//ic void OnInteract()
//    //
//        popupUI.OpenPopup(
//  //        "잠을 자고 하루를 마치//습니까?",
//            () => {
//                //bug.Log("Zzz... 아침이 되었습니다.");
//                // //meManager.Insta//e.NextDay();////       //   }
//        );
//    }

//    public strin//GetInteractPrompt() => "잠자기";
//}