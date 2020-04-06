// EDIT THESE VARIABLES
public const string VERSION = "Template v0.1",
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
public const float SOLAR_TOLERANCE = (float)(Math.PI / 180) * 0.05f; // tenth of a degree
