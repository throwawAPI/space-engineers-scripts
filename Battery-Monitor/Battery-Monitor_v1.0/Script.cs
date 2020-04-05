//
// XXX Public/Battery-Monitor/Battery-Monitor_v1.0/_Customize.cs XXX
//

//
// VARIABLES (EDIT THESE)
//
public const string VERSION = "Battery Monitor v1.0",
  BATTERY_STRING      = ".batteries",
  PANEL_HEADER_STRING = "     Main Base Power Monitor";
public const UpdateFrequency FREQ = UpdateFrequency.Update100;
// set this to true if you don't want to see batteries
// from attached constructs (other ships/stations)
public const bool ONLY_SAME_CONSTRUCT_BATTERIES = true;
public const int PANEL_STD_ROW_COUNT = 23,
  PANEL_WIDE_ROW_COUNT   = 23,
  PANEL_HEADER_ROW_COUNT = 7,
  PANEL_FOOTER_ROW_COUNT = 10;

//
// XXX Public/Battery-Monitor/Battery-Monitor_v1.0/_Main.cs XXX
//

List<IMyTextPanel> lcds;
List<IMyBatteryBlock> batteries;

public Program() {
  Reprogram();
}

public void Reprogram() {
  Runtime.UpdateFrequency = FREQ;
  // Run all the Program__ methods
  Program__Programmable_Block_Display();
  Program__GetBatteries();
  Program__GetLCDs();
  Program__SetupLCDs();
}

public void Save() {
	// Called when the program needs to save its state.
	// Use this method to save your state to the Storage field or some other means.
} // Save()

public void Main(string argument, UpdateType updateSource) {
	// NOTE: multiple triggers can roll in on the same tick
	// Test each one individually

	if((updateSource & UpdateType.Update100) != 0) {
    Main__WriteLCDs();
	} // if trigger via self
  if((updateSource & UpdateType.Trigger) != 0) {
    Reprogram(); // run setup again
  } // if trigger via button or "run" command
  Main__WriteDiagnostics();
} // Main()

public void Program__GetBatteries() {
  batteries = GetBlocksOfTypeWithNames(batteries, BATTERY_STRING)
    .OrderBy(battery => battery.CustomName).ToList();
} // Program__GetBatteries()

public void Program__GetLCDs() {
  lcds = GetBlocksOfTypeWithNames(lcds, BATTERY_STRING)
    .OrderBy(lcd => lcd.CustomName).ToList();
} // Program__GetLCDs()

public void Program__SetupLCDs() {
  foreach (IMyTextPanel lcd in lcds) {
    lcd.ContentType = ContentType.TEXT_AND_IMAGE;
    lcd.Font = "Monospace";
    lcd.FontSize = 0.75f;
  }
} // Program__SetupLCDs()

public void Main__WriteLCDs() {
  int idxBattery = 0;
  foreach(IMyTextPanel lcd in lcds) {
    int idxLine = PANEL_HEADER_ROW_COUNT;
    WriteLCD_Header(lcd, StripClassesFromName(lcd));
    while(idxBattery < batteries.Count) {
      if(idxLine < PANEL_STD_ROW_COUNT) {
        WriteLCD_BatteryInfo(lcd, batteries[idxBattery]);
        idxLine++;
      } else {
        continue; // to next LCD
      }
      idxBattery++;
    } // while idxBattery
    if(idxLine + PANEL_FOOTER_ROW_COUNT > PANEL_STD_ROW_COUNT) {
      continue; // to next LCD
    }
    WriteLCD_Footer(lcd);
  } // foreach lcd
} // Main__WriteLCDs()

public void WriteLCD_Header(IMyTextPanel panel, string customName = null) {
  if(customName == null) {
    customName = panel.CustomName;
  }
  // Display a header, including the screen's name
  panel.WriteText(
    PANEL_HEADER_STRING + HR +
    customName + HR,
    false
    );
}

public void WriteLCD_BatteryInfo(IMyTextPanel screen, IMyBatteryBlock battery) {
  float kWh_f   = battery.CurrentStoredPower * 1000,
    percent_f   = battery.CurrentStoredPower * 100 / battery.MaxStoredPower;
  string kWh_s  = kWh_f.ToString("#,##0").PadLeft(6),
    percent_s   = percent_f.ToString("##0.0").PadLeft(5),
    batteryName = StripClassesFromName(battery);
  // Display the name of a battery block, then the charge level
  screen.WriteText($"{batteryName} = {kWh_s} kWh {percent_s}%\n", true);
}

public void WriteLCD_Footer(IMyTextPanel panel) {
  float inputkW = 0.0f,
    outputkW    = 0.0f,
    storedkWh   = 0.0f,
    maxkWh      = 0.0f,
    percent_f   = 0.0f;
  foreach (IMyBatteryBlock battery in batteries) {
    storedkWh += (battery.CurrentStoredPower * 1000);
    maxkWh    += (battery.MaxStoredPower * 1000);
    inputkW   += (battery.CurrentInput * 1000);
    outputkW  += (battery.CurrentOutput * 1000);
  }
  percent_f = storedkWh * 100 / maxkWh;
  string inputkW_s = inputkW.ToString("#,##0").PadLeft(6),
    outputkW_s     = outputkW.ToString("#,##0").PadLeft(6),
    storedkWh_s    = storedkWh.ToString("#,##0").PadLeft(6),
    maxkWh_s       = maxkWh.ToString("#,##0").PadLeft(6),
    percent_s      = percent_f.ToString("##0.0").PadLeft(6);
  float hourRaw = (inputkW > outputkW) ?
    ((maxkWh - storedkWh)/(inputkW - outputkW)) : (storedkWh/(outputkW - inputkW));
  int secRaw = (int)(hourRaw * 3600),
    hr  = (secRaw / 3600),
    min = (secRaw % 3600) / 60,
    sec = (secRaw % 60);
  string hr_s = hr.ToString("#,##0h:"),
    min_s = min.ToString("00m:"),
    sec_s = sec.ToString("00s"),
    timeUntil = "Time until " + (inputkW > outputkW ? "full" : "empty") + ": " +
    hr_s + min_s + sec_s;
  panel.WriteText(
    HR +
    $"   Station Charge = {percent_s} %\n" +
    $"             {storedkWh_s}/{maxkWh_s} kWh\n" +
    $"            Input = {inputkW_s} kW\n" +
    $"           Output = {outputkW_s} kW" +
    HR + timeUntil + "\n",
    true
    );
  // end?
}

//
// XXX Public/Battery-Monitor/Battery-Monitor_v1.0/_Template_v0.1.cs XXX
//

// XXX START (DO NOT EDIT) XXX
// This portion of the file should not be edited
// Rather, the Template should be revised and re-released with a new version
// Submit a pull-request for edits to the Template

//
// CLASS DEFINITIONS
//
public class Dict<TKey, TValue> : Dictionary<TKey, TValue> {}
public class Dict<TKey1, TKey2, TValue> : Dictionary<TKey1, Dictionary<TKey2, TValue>> {}

public const int PROGRAMMABLE_BLOCK_SCREEN_SURFACE_NUM   = 0; // TODO unverified
public const int PROGRAMMABLE_BLOCK_KEYBOARD_SURFACE_NUM = 1; // TODO unverified
public const float FONT_SIZE_REGULAR = 0.50f;
public const string HR_NO_NL = "====================================", // TODO = or - instead?
HR       = "\n" + HR_NO_NL + "\n",
FONT = "Monospace";
//
// HELPER METHODS
// these are shared across many scripts
// if there is a bug, report it or make a PR on github
// TODO github link
//

// TODO need to be made static?
private void Program__Programmable_Block_Display() {
  IMyTextSurface display = Me.GetSurface(PROGRAMMABLE_BLOCK_SCREEN_SURFACE_NUM);
  string timeString = DateTime.Now.ToString("yyy-MM-dd HH:mm:ss tt");

  // write out to display
  display.ContentType = ContentType.TEXT_AND_IMAGE;
  display.WriteText($"\nCurrent Program: {VERSION}\n", false);
  display.WriteText($"\nLast Compiled: {timeString}\n", true);
} // Program_Programmable_Block_Display()

// TODO need to be made static?
private void Main__WriteDiagnostics() {
  double LastRunTimeNs = Runtime.LastRunTimeMs * 1000;

  // write out to the programmable block's internal console
  Echo($"Last run: {LastRunTimeNs.ToString("n0")}ns");
  Echo($"Instruction limit: ");
  Echo($"{Runtime.CurrentInstructionCount}/{Runtime.MaxInstructionCount}");
} // WriteDiagnostics()

// TODO need to be made static
private string Match(string input, string pattern, string errMsg) {
  System.Text.RegularExpressions.Match match;
  match = System.Text.RegularExpressions.Regex.Match(input, pattern);
  if(match.Success) {
    return match.Value;
  } else {
    Echo(errMsg);
    return (string)(null);
  }
} // Match()

public List<Type> GetBlocksOfTypeWithNames<Type>(List<Type> blocks,
                                                        params string[] strings)
                                                        where Type : class, IMyTerminalBlock {
  // create an empty list of blocks of a specific type
  List<Type> blks = new List<Type>();
  // populate that list with all those blocks
  GridTerminalSystem.GetBlocksOfType(blks);
  // filter that list by the strings provided
  return blocks = FilterBlocksOfTypeWithNames(blks, strings);
}

public List<Type> FilterBlocksOfTypeWithNames<Type>(List<Type> blocks,
                                                            params string[] strings)
                                                            where Type : class, IMyTerminalBlock {
  // find the subset of blocks which match all strings provided
  return blocks = blocks.FindAll(block => strings.All(str => block.CustomName.Contains(str)));
}

public List<IMyTerminalBlock> GetBlocksOfNames(params string[] strings) {
  // create an empty list of blocks of all types
  List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();
  // populate that list with all blocks on the grid
  GridTerminalSystem.GetBlocks(blocks);
  // filter that list by the strings provided
  return FilterBlocksOfTypeWithNames(blocks, strings);
}

public string StripClassesFromName(IMyTerminalBlock blk) {
  string name = blk.CustomName;
  if(name.Contains('.')) {
    name = name.Substring(0, name.IndexOf('.')).Trim();
  }
  return name;
} // StripClassesFromName()

// XXX END (DO NOT EDIT) XXX

