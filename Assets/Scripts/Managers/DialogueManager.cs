using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : Singleton<DialogueManager>
{
    public static event Action<InteractionType> OnExtraInteractionEvent; 
    
    [Header("Config")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private Image npcIcon;
    [SerializeField] private TextMeshProUGUI npcNameTMP;
    [SerializeField] private TextMeshProUGUI npcDialogueTMP;

    public NPCInteraction NPCSelected { get; set; }

    private bool dialogueStarted;
    private PlayerActions actions;
    private Queue<string> dialogueQueue = new Queue<string>();
    
    protected override void Awake()
    {
        base.Awake();
        actions = new PlayerActions();
    }

    private void Start()
    {
        actions.Dialogue.Interact.performed += ctx => ShowDialogue();
        actions.Dialogue.Continue.performed += ctx => ContinueDialogue();
    }

    public void CloseDialoguePanel()
    {
        dialoguePanel.SetActive(false);
        dialogueStarted = false;
        dialogueQueue.Clear();
    }
    
    private void LoadDialogueFromNPC()
    {
        if (NPCSelected.DialogueToShow.Dialogue.Length <= 0) return;
        foreach (string sentence in NPCSelected.DialogueToShow.Dialogue)
        {
            dialogueQueue.Enqueue(sentence);
        }
    }
    
    private void ShowDialogue()
    {
        if (NPCSelected == null) return;
        if (dialogueStarted) return;
        dialoguePanel.SetActive(true);
        LoadDialogueFromNPC();
        npcIcon.sprite = NPCSelected.DialogueToShow.Icon;
        npcNameTMP.text = NPCSelected.DialogueToShow.Name;
        npcDialogueTMP.text = NPCSelected.DialogueToShow.Greeting;
        dialogueStarted = true;
    }

    private void ContinueDialogue()
    {
        if (NPCSelected == null)
        {
            dialogueQueue.Clear();
            return;
        }

        if (dialogueQueue.Count <= 0)
        {
            CloseDialoguePanel();
            dialogueStarted = false;
            if (NPCSelected.DialogueToShow.HasInteraction)
            {
                OnExtraInteractionEvent?
                    .Invoke(NPCSelected.DialogueToShow.InteractionType);
            }
            return;
        }

        npcDialogueTMP.text = dialogueQueue.Dequeue();
    }
    
    private void OnEnable()
    {
        actions.Enable();
    }

    private void OnDisable()
    {
        actions.Disable();
    }
}
