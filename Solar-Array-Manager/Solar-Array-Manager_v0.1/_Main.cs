//// GLOBAL VARIABLES
// for transfering data between Runtime events
enum SolarArrayManagerPhase {
  ScanStart,
  ScanTravelCW,
  ScanTravelCCW,
  ScanTravelMax,
  DialStart,
  DialTravel,
  Night
}
SolarArrayManagerPhase currentPhase = SolarArrayManagerPhase.ScanStart;
List<IMyTextPanel> panels;
List<IMyMotorStator> rotorsAzi, rotorsAlt; // azimuth and alititude
IMyMotorStator masterAzi, masterAlt;
IMySolarPanel masterSolar;
float maxOutAngle, maxOutMW;
bool modeAzimuth = true;

//// Program()
// for variable initialization, setup, etc.
public Program() {
  Runtime.UpdateFrequency = FREQ; // set from _Customize
  Initialize();
} // Program()

public void Initialize() {
  // run each Program__...() submethods here
  Program__Programmable_Block_Display();
  Program__GetSolarRotors();
  Program__GetMasterSolarRotors();
  Program__GetMasterSolarPanel();
  Program__GetLCDs();
  Program__SetupLCDs();
} // Initialize()

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
    Main__RunCurrentPhase();
    Main__WriteLCDs();
  } // if(source & FREQ)
  Main__WriteDiagnostics();
} // Main()

public void Main__RunCurrentPhase() { // TODO document this better
  IMyMotorStator master;
  List<IMyMotorStator> rotors;
  if(modeAzimuth) {
    master = masterAzi;
    rotors = rotorsAzi;
  } else {
    master = masterAlt;
    rotors = rotorsAlt;
  }
  if(master == null) {
    modeAzimuth = !modeAzimuth;
    return; // cannot operate in this mode
  }
  if(masterSolar.CurrentOutput == 0.0f && masterSolar.IsWorking) {
    currentPhase = SolarArrayManagerPhase.Night;
    return; // it's nightime
  }
  switch(currentPhase) {
    case SolarArrayManagerPhase.ScanStart:
      maxOutAngle = master.Angle;
      maxOutMW    = masterSolar.CurrentOutput;
      master.TargetVelocityRPM = 0.01f;
      master.RotorLock = false;
      currentPhase = SolarArrayManagerPhase.ScanTravelCW;
    break;
    case SolarArrayManagerPhase.ScanTravelCW:
      if(masterSolar.CurrentOutput < maxOutMW) {
        master.TargetVelocityRPM = -0.01f;
        currentPhase = SolarArrayManagerPhase.ScanTravelCCW;
      }
      maxOutMW    = masterSolar.CurrentOutput;
      maxOutAngle = master.Angle;
    break;
    case SolarArrayManagerPhase.ScanTravelCCW:
      if(masterSolar.CurrentOutput < maxOutMW) {
        master.TargetVelocityRPM = 0.01f;
        currentPhase = SolarArrayManagerPhase.ScanTravelMax;
      } else {
        maxOutMW    = masterSolar.CurrentOutput;
        maxOutAngle = master.Angle;
      }
    break;
    case SolarArrayManagerPhase.ScanTravelMax:
      if(Math.Abs(master.Angle - maxOutAngle) < SOLAR_TOLERANCE) {
        master.RotorLock = true;
        currentPhase = SolarArrayManagerPhase.DialStart;
      }
    break;
    case SolarArrayManagerPhase.DialStart: // TODO
      currentPhase = SolarArrayManagerPhase.DialTravel;
    break;
    case SolarArrayManagerPhase.DialTravel: // TODO
      modeAzimuth = !modeAzimuth; // alternate between modes
      currentPhase = SolarArrayManagerPhase.ScanStart;
    break;
    case SolarArrayManagerPhase.Night:
      masterAzi.RotorLock = true;
      masterAlt.RotorLock = true;
      if(masterSolar.CurrentOutput > 0.0f) {
        currentPhase = SolarArrayManagerPhase.ScanStart;
      }
    break;
    default:
    // don't know what to do
    break;
  }
} // Main__RunCurrentPhase()

// TODO: make this function look nicer?
public void Main__WriteLCDs() {
  string panelBuffer = SOLAR_LCD_HEADER + HR + "Master Azimuth: ";
  if(masterAzi != null) {
    panelBuffer += StripClassesFromName(masterAzi) + "\n" +
      " - Angle: " + RadiansToDegreesStr(masterAzi.Angle) + "\n";
  } else {
    panelBuffer += "NONE FOUND\n";
  }
  panelBuffer += rotorsAzi.Count + " Azimuth rotor(s) total" + HR +
    "Master Altitude: ";
  if(masterAlt != null) {
    panelBuffer += StripClassesFromName(masterAlt) + "\n" +
      " - Angle: " + RadiansToDegreesStr(masterAlt.Angle) + "\n";
  } else {
    panelBuffer += "NONE FOUND\n";
  }
  panelBuffer += rotorsAlt.Count + " Altitude rotor(s) total" + HR +
    "Master Solar: ";
  if(masterSolar != null) {
    panelBuffer += StripClassesFromName(masterSolar) + "\n" +
      " - Power: " + MWToKWStr(masterSolar.CurrentOutput) + " / " +
      MWToKWStr(maxOutMW) + " kW\n" +
      " - Angle: " + RadiansToDegreesStr(maxOutAngle);
  } else {
    panelBuffer += "NONE FOUND";
  }
  panelBuffer += HR + "Current Phase: " + currentPhase.ToString("G");
  foreach (IMyTextPanel panel in panels) {
    panel.WriteText(panelBuffer, false);
  }
} // Main__WriteLCDs()

public string RadiansToDegreesStr(double radians) {
  return ((radians * 180) / Math.PI).ToString("##0.0");
} // RadiansToDegreesStr()

public string MWToKWStr(float MW) {
  return (MW * 1000).ToString("#,##0.00");
} // MWToKWStr()
