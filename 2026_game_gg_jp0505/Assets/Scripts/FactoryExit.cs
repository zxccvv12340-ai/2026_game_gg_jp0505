using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(BoxCollider2D))]
public sealed class FactoryExit : MonoBehaviour
{
    [SerializeField] private TextMeshPro prompt;
    [SerializeField] private DialogueBox dialogue;
    [SerializeField] private SpriteRenderer marker;
    [SerializeField] private string destination;

    private readonly HashSet<Collider2D> playersInRange = new();
    private bool loading;
    public bool IsUnlocked => PowerQuest.Current >= PowerQuest.Progress.Accepted;

    private void Awake() => RefreshPrompt();

    private void Start()
    {
        if (!PowerQuest.ReturningToTown)
            return;

        PowerQuest.FinishReturn();
        var player = GameObject.FindWithTag("Player");
        if (player != null && player.TryGetComponent<Rigidbody2D>(out var body))
            body.position = (Vector2)transform.position + Vector2.up * 1.6f;
    }

    public void Unlock()
    {
        PowerQuest.Accept();
        RefreshPrompt();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.attachedRigidbody != null && other.attachedRigidbody.CompareTag("Player"))
        {
            playersInRange.Add(other);
            RefreshPrompt();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (playersInRange.Remove(other))
            RefreshPrompt();
    }

    private void RefreshPrompt()
    {
        prompt.gameObject.SetActive(playersInRange.Count > 0);
        prompt.text = IsUnlocked ? "[E] 폐공장 입장" : "폐공장 · 잠김";
        marker.color = IsUnlocked ? Color.white : new Color(0.72f, 0.55f, 0.52f);
    }

    private void LateUpdate()
    {
        if (loading || !IsUnlocked || playersInRange.Count == 0 || dialogue.isOpen)
            return;

        // An NPC and an exit use the same interaction key.
        if (dialogue.nearTalk != null || Keyboard.current?.eKey.wasPressedThisFrame != true)
            return;

        int buildIndex = SceneUtility.GetBuildIndexByScenePath(destination);
        if (buildIndex < 0)
        {
            Debug.LogError($"Factory scene is missing from the build scene list: {destination}", this);
            return;
        }

        loading = SceneManager.LoadSceneAsync(buildIndex) != null;
    }
}
