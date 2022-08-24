using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;
using TMPro;

public class CommandManager : MonoBehaviour
{
    public static bool IsCommandWindowOpen = false;

    [SerializeField] private GameObject commandWindow;

    private const char ARGUMENT_SEPERATOR = ',';
    private const char COMMAND_START = '/';
    private const char COMMAND_END = '.';
    private static readonly string COMMAND_INFO ="**Commands are not space or case sensitive." +
                                                                            $"\n**Commands start with [ {COMMAND_START} ]"+
                                                                            $"\n**Commands end with [ {COMMAND_END} ]" +
                                                                            $"\n**Multiple arguments are seperated by [ {ARGUMENT_SEPERATOR} ]";

    private readonly Dictionary<string, System.Action> commands = new Dictionary<string, System.Action>();
    private readonly List<CommandInput> commandInputs = new List<CommandInput>();

    private bool openWindow = false;
    private TextMeshProUGUI commandLogs;
    private TMP_InputField commandText;
    private Player player;
    private PlayerController playerController;

    private void Awake()
    {
        AddCommand("tp", Com_tp, "x, y, z");
        AddCommand("collider", Com_collider, "on/off");
        AddCommand("immortal", () => OnOffCommand(ParseArgs(), () => Player.IsImmortal = true, () => Player.IsImmortal = false), "on/off");
        AddCommand("add", Com_add, "itemName, quantity");
        AddCommand("fly", Com_fly, "on/off");
        AddCommand("movespeed", Com_movespeed, "get/set, moveSpeedValue");
        AddCommand("flyspeed", Com_flyspeed, "get/set, flySpeedValue");

        //non-gameplay commands
        AddCommand("exit", Application.Quit, string.Empty);
        AddCommand("help", Com_help, string.Empty);

        player = FindObjectOfType<Player>();
        playerController = FindObjectOfType<PlayerController>();
        commandText = commandWindow.transform.GetChild(0).GetComponent<TMP_InputField>();
        commandLogs = commandWindow.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
        commandWindow.SetActive(false);

        commandText.onEndEdit.AddListener(delegate { ExecuteCommand(); } );
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Return))
        {
            if (commandWindow.activeSelf) SetWindowVisible(false);
            else SetWindowVisible(true);
        }

        if (openWindow) 
        {
            openWindow = false;
            SetWindowVisible(true);
        }
    }

  
    private void SetWindowVisible(bool visible) 
    {
        IsCommandWindowOpen = visible;

        if (visible == false)
        {
            commandWindow.SetActive(false);
            EventSystem.current.SetSelectedGameObject(null);
            PlayerController.SetCursor(CursorLockMode.Locked, false);
        }
        else
        {
            commandWindow.SetActive(true);
            PlayerController.SetCursor(CursorLockMode.None, true);
            EventSystem.current.SetSelectedGameObject(commandText.gameObject);
        }
    }

    public void ExecuteCommand()
    {
        commandLogs.text = string.Empty;
        try
        {
            string com = commandText.text.ToLower().Replace(" ", "");
            if (string.IsNullOrEmpty(com)) return;

            if (com[0] == COMMAND_START && com.Contains("help")) 
            {
                Com_help();
                return;
            }

            int start = com.IndexOf(COMMAND_START);
            int end = com.IndexOf(COMMAND_END);
            com = com.Substring(start + 1, end - 1);

            if (commands.ContainsKey(com)) 
            {
                Log(commandText.text);
                commands[com]?.Invoke(); 
            }
        }
        catch { Log("Invalid command!"); }
    }

    private string ParseArgs()
    {
        string com = commandText.text.Replace(" ", "").ToLower();
        if (com[0] != COMMAND_START) return string.Empty;
        string[] args = com.Split(COMMAND_END);
        return args[1];
    }

    #region Commands

    private void Com_movespeed() 
    {
        string[] args = ParseArgs().Split(ARGUMENT_SEPERATOR);
        if (args == null || args.Length < 1) 
        {
            Log("Invalid Arguments.");
            return;
        }

        if (args[0] == "set")
        {
            float.TryParse(args[1], out float s);
            FindObjectOfType<PlayerController>().SetMoveSpeed(s);
        }
        else if (args[0] == "get")
        {
            Log(FindObjectOfType<PlayerController>().GetMoveSpeed().ToString());
        }

    }

    private void Com_flyspeed()
    {
        string[] args = ParseArgs().Split(ARGUMENT_SEPERATOR);
        if (args == null || args.Length < 1)
        {
            Log("Invalid Arguments.");
            return;
        }

        if (args[0] == "set")
        {
            float.TryParse(args[1], out float s);
            FindObjectOfType<PlayerController>().SetFlySpeed(s);
        }
        else if (args[0] == "get")
        {
            Log(FindObjectOfType<PlayerController>().GetFlySpeed().ToString());
        }

    }

    private void Com_fly()
    {
        static void pos() { FindObjectOfType<PlayerController>().SetControlMode(PlayerController.ControlMode.Fly); }
        static void neg() { FindObjectOfType<PlayerController>().SetControlMode(PlayerController.ControlMode.Normal); }
        OnOffCommand(ParseArgs(), pos, neg);
    }

    private void Com_add() 
    {
        string args = ParseArgs();
        int count = 1;

        string[] parsedArgs = ParseArgs().Split(ARGUMENT_SEPERATOR);
        ItemData itemData = ItemManager.Instance.GetItemData(parsedArgs.Length >= 2 ? parsedArgs[0] : args);
        if (parsedArgs.Length >= 2) int.TryParse(parsedArgs[1], out count);

        if (itemData != null)
           for(int i = 0; i < (count <= 0 ?  1 : count) ; i++) 
                player.GetComponent<Interactor>().AddToInventory(new Item(itemData), null);
    }
    
    private void Com_help() 
    {
        Log("\n" + COMMAND_INFO + "\n");
        foreach (CommandInput input in commandInputs)
            foreach (string arg in input.arguments)
                Log($"/{input.command}. {arg}");

        Log("----------------------------------------------------------------------------------------------");
        openWindow = true;
    }

    private void Com_tp()
    {
        string[] args = ParseArgs().Split(ARGUMENT_SEPERATOR);
        float.TryParse(args[0], out float x);
        float.TryParse(args[1], out float y);
        float.TryParse(args[2], out float z);
        player.GetComponent<PlayerController>().TeleportTo(new Vector3(x, y, z));
    }

    private void Com_collider() 
    {
        string arg = ParseArgs();
        OnOffCommand(arg,
            () => playerController.GetComponent<Collider>().enabled = true,
            () => playerController.GetComponent<Collider>().enabled = false) ;
    }

    #endregion

    public void Log(string msg) 
    {
        commandLogs.text += msg + "\n";
    }

    private void AddCommand(string com, System.Action action, params string[] args) 
    {
        commands.Add(com, action);
        commandInputs.Add(new CommandInput(com, args));
    }
    
    private void OnOffCommand(string com, System.Action positive, System.Action negative) 
    {
        if (com == "on") positive?.Invoke();
        else if (com == "off") negative?.Invoke();
    }

    private struct CommandInput 
    {
        public readonly string command;
        public readonly string[] arguments;

        public CommandInput(string command, string[] arguments) 
        {
            this.command = command;
            this.arguments = arguments;
        }
    }
}


