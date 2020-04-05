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
