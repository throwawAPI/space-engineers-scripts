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
