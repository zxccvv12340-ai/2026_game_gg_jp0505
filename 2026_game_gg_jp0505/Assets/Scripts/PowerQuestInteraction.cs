using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(BoxCollider2D))]
public sealed class PowerQuestInteraction : MonoBehaviour
{
    private enum Action { Battery, ReturnToTown, Generator }

    [SerializeField] private Action action;
    [SerializeField] private TextMeshPro prompt;
    [SerializeField] private DialogueBox dialogue;
    [SerializeField] private GameObject poweredIndicator;
    [SerializeField] private Light2D townLight;
    [SerializeField] private string townScenePath;

    private readonly HashSet<Collider2D> playersInRange = new();
    private bool loading;
    private float restoredIntensity;

    private void Awake()
    {
        if (action == Action.Battery && PowerQuest.Current >= PowerQuest.Progress.CarryingBattery)
        {
            gameObject.SetActive(false);
            return;
        }

        if (townLight != null)
            restoredIntensity = townLight.intensity;
        Refresh();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.attachedRigidbody != null && other.attachedRigidbody.CompareTag("Player"))
        {
            playersInRange.Add(other);
            Refresh();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (playersInRange.Remove(other))
            Refresh();
    }

    private void LateUpdate()
    {
        if (loading || playersInRange.Count == 0 || Keyboard.current?.eKey.wasPressedThisFrame != true)
            return;
        if (dialogue != null && (dialogue.isOpen || dialogue.nearTalk != null))
            return;

        switch (action)
        {
            case Action.Battery:
                PowerQuest.PickUpBattery();
                gameObject.SetActive(false);
                break;
            case Action.ReturnToTown:
                ReturnToTown();
                break;
            case Action.Generator:
                PowerQuest.Repair();
                Refresh();
                break;
        }
    }

    private void ReturnToTown()
    {
        int index = SceneUtility.GetBuildIndexByScenePath(townScenePath);
        if (index < 0)
        {
            Debug.LogError($"Town scene is missing from the build scene list: {townScenePath}", this);
            return;
        }
        loading = SceneManager.LoadSceneAsync(index) != null;
        if (loading)
            PowerQuest.BeginReturn();
    }

    private void Refresh()
    {
        prompt.gameObject.SetActive(playersInRange.Count > 0);
        switch (action)
        {
            case Action.Battery:
                prompt.text = "[E] 예비 배터리 획득";
                break;
            case Action.ReturnToTown:
                prompt.text = PowerQuest.Current == PowerQuest.Progress.CarryingBattery
                    ? "배터리 획득!  [E] 마을로 돌아가기"
                    : "[E] 마을로 돌아가기";
                break;
            case Action.Generator:
                bool repaired = PowerQuest.Current == PowerQuest.Progress.Repaired;
                poweredIndicator.SetActive(repaired);
                townLight.intensity = repaired ? restoredIntensity : restoredIntensity * 0.68f;
                prompt.text = PowerQuest.Current switch
                {
                    PowerQuest.Progress.CarryingBattery => "[E] 배터리 설치",
                    PowerQuest.Progress.Repaired => "임무 완료! 마을에 전기가 돌아왔다.",
                    _ => "고장 난 발전기 · 예비 배터리가 필요하다."
                };
                break;
        }
    }
}
