// thrust.js
// build for use with /space-engineers-scripts/thrust.html
// but should be easy enough to repurpose as needed

class SpaceEngineers {
  static Planet = class Planet {
    constructor(template) {
      for(let key in template) {
        this[key] = template[key];
      }
    } // constructor()

    getGravity_f(height_km) {
      if(height_km < this.gravityStart_km) {
        return this.gravity_g;
      } else if(height_km > this.gravityEnd_km) {
        return 0.0;
      } else {
        return (this.gravity_g * (height_km - this.gravityStart_km) / (this.gravityEnd_km - this.gravityStart_km));
      }
    } // getGravity_f()

    getThruster_kN(height_km, type_str, blockSize_str, largeVarient_str) {
      let efficiency_f = 0.0;
      let maxThrust_kN = SpaceEngineers.ThrusterTable[type_str][blockSize_str][largeVarient_str];
      switch (type_str) {
        case "atmospheric":
          efficiency_f = (this.atmoDensity_f * (1.0 - (Math.min(this.atmoEnd_km, height_km) / this.atmoEnd_km))) || 0;
          break;
        case "hydrogen":
          return maxThrust_kN;
          break;
        case "ion":
          efficiency_f = (0.3 + (0.7 * Math.min(this.ionEnd_km, height_km) / this.ionEnd_km)) || 1;
          break;
        default:
          return 0.0;
      }
      return efficiency_f * maxThrust_kN;
    } // getThruster_kN

    static Earth = new Planet({
      gravity_g :        1.0, // gravity at surface
      atmoDensity_f :    1.0, // density of atmosphere at surface
      gravityStart_km :  6.0, // height at which gravity begins falling
      gravityEnd_km :   42.0, // height at which gravity < 0.05g
      atmoEnd_km :      10.0, // height at which atmo thrusters fall to 0
      ionEnd_km :       20.0, // height at which ion thrusters begin falling
      name : "Earth"
    }); // Earth template

    static Moon = new Planet({
      gravity_g :       0.25,
      atmoDensity_f :   0.0,
      gravityStart_km : 0.0,
      gravityEnd_km :   3.0,
      atmoEnd_km :      0.0,
      ionEnd_km :       0.0,
      name : "Moon"
    }); // Moon template

    static Mars = new Planet({
      gravity_g :       0.9,
      atmoDensity_f :   0.8,
      gravityStart_km : 6.0,
      gravityEnd_km :  42.0,
      atmoEnd_km :     10.0,
      ionEnd_km :      20.0,
      name : "Mars"
    }); // Mars template
  } // class Planet

  static ThrusterTable = {
    atmospheric : {
      gridSmall : {
        small :  96.0,
        large : 576.0
      },
      gridLarge : {
        small :  648.0,
        large : 6480.0
      }
    },
    hydrogen : {
      gridSmall : {
        small :  98.4,
        large : 480.0
      },
      gridLarge : {
        small : 1080.0,
        large : 7200.0
      }
    },
    ion : {
      gridSmall : {
        small :  14.4,
        large : 172.8,
      },
      gridLarge : {
        small :  345.6,
        large : 4320.0
      }
    }
  } // ThrusterTable
} // class SpaceEngineers
