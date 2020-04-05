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
