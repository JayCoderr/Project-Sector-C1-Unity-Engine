using Photon.Realtime;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class bl_RoomChat : bl_RoomChatBase
{
    public int messageBufferLenght = 7;
    [Header("References")]
    public GameObject chatUIRoot;
    public TMP_InputField chatInputField;
    public TextMeshProUGUI chatText;

    private readonly List<string> messages = new();
    private MessageTarget messageTarget = MessageTarget.All;
    private bool isChatInputBlocked = false;

    /// <summary>
    /// 
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        bl_GameData.isChatting = false;
        this.InvokeAfter(5, HideChat);
        if (chatInputField != null) chatInputField.gameObject.SetActive(false);
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void OnEnable()
    {
        base.OnEnable();
        bl_PhotonNetwork.Instance.AddCallback(PropertiesKeys.ChatEvent, OnNetworkMessageReceived);
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void OnDisable()
    {
        base.OnDisable();
        if (bl_PhotonNetwork.Instance != null)
            bl_PhotonNetwork.Instance.RemoveCallback(OnNetworkMessageReceived);
    }

    /// <summary>
    /// 
    /// </summary>
    private void Start()
    {
#if LOCALIZATION
        SetChatLocally(bl_Localization.Instance.GetText(29));
#else
        SetChatLocally(bl_GameTexts.OpenChatStart);
#endif
    }

    /// <summary>
    /// 
    /// </summary>
    public override void OnUpdate()
    {
        if (!CanUseTheChat || isChatInputBlocked) return;

        if (!bl_GameData.isChatting)
        {
            if (bl_GameInput.GeneralChat())
            {
                messageTarget = MessageTarget.All;
                OnSubmit();
            }
            if (bl_GameInput.TeamChat())
            {
                messageTarget = MessageTarget.Team;
                OnSubmit();
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                OnSubmit();
            }
        }
    }

    /// <summary>
    /// Receive network message from chat event from the server
    /// </summary>
    /// <param name="data"></param>
    void OnNetworkMessageReceived(ExitGames.Client.Photon.Hashtable data)
    {
        var messageTarget = (MessageTarget)data["target"];
        var sender = (Player)data["sender"];
        var message = (string)data["chat"];

        if (messageTarget == MessageTarget.Team && sender.GetPlayerTeam() != bl_PhotonNetwork.LocalPlayer.GetPlayerTeam())//check if team message and only team receive it
            return;

        string senderName = "anonymous";

        if (sender != null)
        {
            senderName = string.IsNullOrEmpty(sender.NickName) ? $"Player {sender.ActorNumber}" : sender.NickName;
        }

        // apply filters
        message = bl_Hook.ApplyFilters("PreChatMessage", message);

        bl_Hook.DoAction("OnChatMessage", message, senderName);

        string txt = string.Format("[{0}] [{1}]:{2}", messageTarget.ToString().ToUpper(), senderName, message);
        this.messages.Add(txt);
        if (messages.Count > messageBufferLenght)
        {
            messages.RemoveAt(0);
        }

        chatText.text = string.Empty;
        foreach (string m in messages)
        {
            chatText.text += m + "\n";
        }
        CancelInvoke(nameof(HideChat));
        chatText.CrossFadeAlpha(1, 0.3f, true);
        Invoke(nameof(HideChat), 5);
    }

    /// <summary>
    /// Send text message over network to all other clients
    /// </summary>
    /// <param name="txt"></param>
    public override void SetChat(string txt)
    {
        if (!bl_PhotonNetwork.IsConnected || !CanUseTheChat)
            return;
        if (string.IsNullOrEmpty(txt)) return;

        if (bl_GameData.CoreSettings.filterProfanityWords)
        {
            if (bl_StringUtility.ContainsProfanity(txt, out string filterText))
            {
                txt = filterText;
            }
        }

        var data = bl_UtilityHelper.CreatePhotonHashTable();
        data.Add("sender", bl_PhotonNetwork.LocalPlayer);
        data.Add("target", messageTarget);
        data.Add("chat", txt);

        bl_PhotonNetwork.Instance.SendDataOverNetwork(PropertiesKeys.ChatEvent, data);
    }

    /// <summary>
    /// Post a message in the chat locally only.
    /// </summary>
    /// <param name="newLine"></param>
    public override void SetChatLocally(string newLine)
    {
        this.messages.Add(newLine);
        if (messages.Count > messageBufferLenght)
        {
            messages.RemoveAt(0);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="active"></param>
    public override void SetActiveUI(bool active)
    {
        chatInputField.gameObject.SetActive(active);
    }

    /// <summary>
    /// 
    /// </summary>
    public override void OnSubmit()
    {
        bl_GameData.isChatting = !bl_GameData.isChatting;
        chatInputField.gameObject.SetActive(bl_GameData.isChatting);

        if (bl_GameData.isChatting)
        {
            chatInputField.text = string.Empty;
            chatInputField.ActivateInputField();
        }
        else
        {
            SetChat(chatInputField.text);
            chatInputField.DeactivateInputField();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void HideChat()
    {
        chatText.CrossFadeAlpha(0, 2, true);
    }

    /// <summary>
    /// 
    /// </summary>
    public void Refresh()
    {
        chatText.text = string.Empty;
        foreach (string m in messages)
            chatText.text += m + "\n";
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="block"></param>
    public override void BlockChat(bool block)
    {
        isChatInputBlocked = block;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="field"></param>
    public void SubmitInputField()
    {
        OnSubmit();
    }

    public enum MessageTarget
    {
        All,
        Team,
    }
}