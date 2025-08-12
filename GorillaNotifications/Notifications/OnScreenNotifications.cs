using System.Collections;
using UnityEngine;

namespace GorillaNotifications.Notifications
{
    internal class OnScreenNotifications : MonoBehaviour
    {
        public static OnScreenNotifications Instance { get; private set; }

        private string currentNotification = "hi\nsupercool";
        private float currentDuration;
        
        private Coroutine notificationCoroutine;
        
        private GUIStyle labelStyle;
        
        private void Start()
        {
            labelStyle = new GUIStyle();
            labelStyle.alignment = TextAnchor.MiddleCenter;
            labelStyle.richText = true;
            labelStyle.fontSize = 40;
            labelStyle.normal.textColor = Color.white;
        }

        public void SendNotification(string notification, float duration)
        {
            currentNotification = notification;
            currentDuration = duration;
            
            if (notificationCoroutine != null)
                StopCoroutine(notificationCoroutine);
            
            notificationCoroutine = StartCoroutine(DisplayNotification());
        }

        private IEnumerator DisplayNotification()
        {
            yield return new WaitForSeconds(currentDuration);
            currentNotification = "";
        }

        private void OnGUI() => GUI.Label(new Rect(new Vector2(Screen.width/4f, Screen.height - 110f), new Vector2(Screen.width/2f, 100f)), currentNotification, labelStyle);
        private void Awake() => Instance = this;
    }
}