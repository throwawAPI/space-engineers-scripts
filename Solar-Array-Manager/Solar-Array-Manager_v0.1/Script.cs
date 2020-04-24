//
// XXX Solar-Array-Manager/Solar-Array-Manager_v0.1/_Customize.cs XXX
//

// EDIT THESE VARIABLES
public const string VERSION = "Solar Array Manager v0.2",
  SOLAR_LCD_HEADER = "      SOLAR ARRAY MANAGER",
  SOLAR_MANAGED    = ".solar",
  SOLAR_MASTER     = ".master",
  SOLAR_ROTOR_AZI  = ".azimuth",
  SOLAR_ROTOR_ALT  = ".altitude",
  SOLAR_OFFSET_0   = ".angle0",
  SOLAR_OFFSET_90  = ".angle90",
  SOLAR_OFFSET_180 = ".angle180",
  SOLAR_OFFSET_270 = ".angle270";
public const UpdateFrequency FREQ = UpdateFrequency.Update100;
public const float SOLAR_ANGLE_TOLERANCE = (float)(Math.PI / 180) * 0.1f, // 0.1º
  SOLAR_SPEED_TARGET_RPM = 0.01f,
  SOLAR_WATT_TOLERANCE  = 0.00001f, // 0.05kW
  SOLAR_DAY_POWER = 0.001f; // 1kW

//
// XXX Solar-Array-Manager/Solar-Array-Manager_v0.1/_Main.cs XXX
//

//// GLOBAL VARIABLES
// for transfering data between Runtime events
IEnumerator<SolarPhase> phase;
List<IMyMotorStator> rotorsAzi, rotorsAlt; // azimuth and alititude
List<IMyTextPanel> panels;
IMyMotorStator masterAzi, masterAlt;
IMySolarPanel masterSolar;
float maxOverallAngle, maxOverallMW, maxPhaseMW;
bool modeAzimuth = true;
int speedMult = 0;

public enum SolarPhase {
  MasterStart,
  MasterFirst,
  MasterFirstTravel,
  MasterSecond,
  MasterSecondTravel,
  SlaveStart,
  SlaveTravel
}

//// Program()
// for variable initialization, setup, etc.
public Program() {
  Runtime.UpdateFrequency = FREQ; // set from _Customize
  Initialize();
} // Program()

public void Initialize() {
  // run each Program__...() submethods here
  Program__Programmable_Block_Display();
  Program__GetPhase();
  Program__GetSolarRotors();
  Program__GetMasterSolarRotors();
  Program__GetMasterSolarPanel();
  Program__GetLCDs();
  Program__SetupLCDs();
} // Initialize()

public void Program__GetPhase() {
  phase = SolarPhases();
} // Program__GetPhase()

public void Program__GetSolarRotors() {
  rotorsAzi = GetBlocksOfTypeWithNames(rotorsAzi, SOLAR_MANAGED, SOLAR_ROTOR_AZI)
    .FindAll(rotor => rotor.IsSameConstructAs(Me));
  rotorsAlt = GetBlocksOfTypeWithNames(rotorsAlt, SOLAR_MANAGED, SOLAR_ROTOR_ALT)
    .FindAll(rotor => rotor.IsSameConstructAs(Me));
} // Program__GetSolarRotors()

public void Program__GetMasterSolarRotors() {
  List<IMyMotorStator> rotorsTemp;
  rotorsTemp = FilterBlocksOfTypeWithNames(rotorsAzi, SOLAR_MASTER);
  if(rotorsTemp.Count > 0) {
    masterAzi = rotorsTemp[0];
  }
  rotorsTemp = FilterBlocksOfTypeWithNames(rotorsAlt, SOLAR_MASTER);
  if(rotorsTemp.Count > 0) {
    masterAlt = rotorsTemp[0];
  }
} // Program__GetMasterSolarRotors()

public void Program__GetMasterSolarPanel() {
  List<IMySolarPanel> solars = null;
  masterSolar = GetBlocksOfTypeWithNames(solars, SOLAR_MANAGED, SOLAR_MASTER)[0];
} // Program__GetMasterSolarPanel()

public void Program__GetLCDs() {
  panels = GetBlocksOfTypeWithNames(panels, SOLAR_MANAGED)
    .FindAll(panel => panel.IsSameConstructAs(Me))
    .OrderBy(panel => panel.CustomName).ToList();
} // Program__GetLCDs()

public void Program__SetupLCDs() {
  foreach (IMyTextPanel panel in panels) {
    panel.ContentType = ContentType.TEXT_AND_IMAGE;
    panel.Font = "Monospace";
    panel.FontSize = 0.75f;
  }
} // Program__SetupLCDs()

//// Save()
// called when the Programmable Block shuts down
// use this method to save state to the storage field
public void Save() {
} // Save()

//// Main()
// called when the Programmable Block is "Run",
// or automatically by UpdateFrequency
public void Main(string arg, UpdateType source) {
  // NOTE: multiple trigger sources can roll in on the same tick
  // test each trigger individually, not with if() else if () blocks

  if((source & UpdateType.Update100) != 0) {
    Main__WriteLCDs();
    Main__RunNextPhase();
  } // if(source & FREQ)
  Main__WriteDiagnostics();
} // Main()

public void Main__RunNextPhase() { // TODO document this better
  IMyMotorStator master;
  List<IMyMotorStator> rotors;

  if(modeAzimuth) {
    master = masterAzi;
    rotors = rotorsAzi;
  } else {
    master = masterAlt;
    rotors = rotorsAlt;
  }
  // if we don't have a master, retry with the other mode
  if(master == null) {
    modeAzimuth = !modeAzimuth;
    return;
  }

  if(masterSolar.CurrentOutput > maxOverallMW) {
    maxOverallAngle = master.Angle;
    maxOverallMW    = masterSolar.CurrentOutput;
  }

  // return to base position
  if(masterSolar.CurrentOutput < SOLAR_DAY_POWER && masterSolar.IsWorking) {
    if(masterAzi != null) {
      Main__RotorSetAngleOrLock(masterAzi, 0);
    }
    if(masterAlt != null) {
      Main__RotorSetAngleOrLock(masterAzi, 0);
    }
    phase = SolarPhases();
    return;
  }

  if(phase.Current == SolarPhase.MasterFirst) {
    speedMult = 10;
  } else if(phase.Current == SolarPhase.MasterSecond) {
    speedMult = 1;
  }

  switch(phase.Current) {
    case SolarPhase.MasterStart: // assume we're at the best angle
      modeAzimuth = !modeAzimuth; // alternate between modes
      maxOverallAngle = master.Angle;
      maxOverallMW    = masterSolar.CurrentOutput;
      master.TargetVelocityRPM = SOLAR_SPEED_TARGET_RPM;
      phase.MoveNext();
      break;
    case SolarPhase.MasterFirst:
    case SolarPhase.MasterSecond:
      maxPhaseMW = masterSolar.CurrentOutput;
      master.TargetVelocityRPM = -1 * Math.Sign(master.TargetVelocityRPM) * SOLAR_SPEED_TARGET_RPM * speedMult; // reverse
      master.RotorLock = false;
      phase.MoveNext();
      break;
    case SolarPhase.MasterFirstTravel:
    case SolarPhase.MasterSecondTravel:
      if(masterSolar.CurrentOutput < maxPhaseMW - (SOLAR_WATT_TOLERANCE * speedMult)) {
        master.RotorLock = true;
        phase.MoveNext();
      } else if(masterSolar.CurrentOutput > maxPhaseMW) {
        maxPhaseMW = masterSolar.CurrentOutput;
      }
      break;
    case SolarPhase.SlaveStart: // TODO
      foreach(IMyMotorStator rotor in rotors) {
        Main__RotorSetAngleOrLock(rotor, maxOverallAngle);
      }
      phase.MoveNext();
      break;
    case SolarPhase.SlaveTravel: // TODO
      bool stopped = true;
      foreach (IMyMotorStator rotor in rotors) {
        Main__RotorSetAngleOrLock(rotor, maxOverallAngle);
        stopped = stopped && rotor.RotorLock;
      }
      if(stopped) {
        phase.MoveNext();
      }
      break;
    default: // don't know what to do
      break;
  }
} // Main__RunNextPhase()

public void Main__WriteLCDs() {
  foreach (IMyTextPanel panel in panels) {
    panel.WriteText(SOLAR_LCD_HEADER, false);
    WriteLCD__Rotor(panel, "Azimuth",  masterAzi, rotorsAzi);
    WriteLCD__Rotor(panel, "Altitude", masterAlt, rotorsAlt);
    WriteLCD__SolarPanel(panel, masterSolar);
    panel.WriteText(HR + "Current Phase: " + phase.Current.ToString("G"), true);
  }
} // Main__WriteLCDs()

public void Main__RotorSetAngleOrLock(IMyMotorStator rotor, float targetAngle) {
  if(Math.Abs(rotor.Angle - targetAngle) < SOLAR_ANGLE_TOLERANCE) {
    rotor.RotorLock = true;
  } else {
    rotor.RotorLock = false;
    rotor.TargetVelocityRPM = (float)(targetAngle - rotor.Angle); // TODO: this travel arc is suboptimal
    if(Math.Abs(rotor.TargetVelocityRPM) < SOLAR_SPEED_TARGET_RPM) {
      rotor.TargetVelocityRPM = Math.Sign(rotor.TargetVelocityRPM) * SOLAR_SPEED_TARGET_RPM;
    }
  }
} // Main__RotorSetAngleOrLock()

public void WriteLCD__Rotor(IMyTextPanel panel, string rotorName, IMyMotorStator rotor, List<IMyMotorStator> rotors) {
  string buffer = HR + "Master " + rotorName + ": ";
  if(rotor != null) {
    buffer += StripClassesFromName(rotor) + "\n" +
      " - Angle:  " + RadiansToDegreesStr(rotor.Angle) + "°\n" +
      " - Speed:  " + rotor.TargetVelocityRPM + " RPM\n" +
      " - Locked: " + rotor.RotorLock + "\n";
  } else {
    buffer += "NONE\n";
  }
  buffer += rotors.Count + " " + rotorName + " rotor(s):";
  foreach (IMyMotorStator slaveRotor in rotors) {
    buffer += "\n > " + StripClassesFromName(slaveRotor) + " @ " + RadiansToDegreesStr(slaveRotor.Angle) + "°";
  }
  panel.WriteText(buffer, true);
} // WriteLCD__Rotor()

public void WriteLCD__SolarPanel(IMyTextPanel panel, IMySolarPanel solar) {
  string buffer = HR + "Master Solar: ";
  if(solar != null) {
    buffer += StripClassesFromName(solar) + "\n" +
      " - Power: " + MWToKWStr(solar.CurrentOutput) + " kW\n" +
      "          " + MWToKWStr(maxOverallMW) + " kW (Max)\n" +
      " - Angle: " + RadiansToDegreesStr(maxOverallAngle) + "° (Max)";
  } else {
    buffer += "NONE";
  }
  panel.WriteText(buffer, true);
} // WriteLCD__SolarPanel()

public IEnumerator<SolarPhase> SolarPhases() {
  while(true) {
    yield return SolarPhase.MasterStart;
    yield return SolarPhase.MasterFirst;
    yield return SolarPhase.MasterFirstTravel;
    yield return SolarPhase.MasterSecond;
    yield return SolarPhase.MasterSecondTravel;
    yield return SolarPhase.SlaveStart;
    yield return SolarPhase.SlaveTravel;
  }
} // SolarPhases()

public string RadiansToDegreesStr(double radians) {
  return ((radians * 180) / Math.PI).ToString("##0.0");
} // RadiansToDegreesStr()

public string MWToKWStr(float MW) {
  return (MW * 1000).ToString("#,##0.00");
} // MWToKWStr()

//
// XXX Solar-Array-Manager/Solar-Array-Manager_v0.1/_Template_v0.1.cs XXX
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

private void Program__Programmable_Block_Display() {
  IMyTextSurface display = Me.GetSurface(PROGRAMMABLE_BLOCK_SCREEN_SURFACE_NUM);
  string timeString = DateTime.Now.ToString("yyy-MM-dd HH:mm:ss tt");

  // write out to display
  display.ContentType = ContentType.TEXT_AND_IMAGE;
  display.WriteText($"\nCurrent Program: {VERSION}\n", false);
  display.WriteText($"\nLast Compiled: {timeString}\n", true);
} // Program__Programmable_Block_Display()

private void Main__WriteDiagnostics() {
  double LastRunTimeNs = Runtime.LastRunTimeMs * 1000;

  // write out to the programmable block's internal console
  Echo($"Last run: {LastRunTimeNs.ToString("n0")}ns");
  Echo($"Instruction limit: ");
  Echo($"{Runtime.CurrentInstructionCount}/{Runtime.MaxInstructionCount}");
} // WriteDiagnostics()

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
  return FilterBlocksOfTypeWithNames(blks, strings);
}

public List<Type> FilterBlocksOfTypeWithNames<Type>(List<Type> blocks,
                                                            params string[] strings)
                                                            where Type : class, IMyTerminalBlock {
  // find the subset of blocks which match all strings provided
  return blocks.FindAll(block => strings.All(str => block.CustomName.Contains(str)));
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

