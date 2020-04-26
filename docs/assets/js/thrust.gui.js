// thrust.gui.js

class ThrustGUI {
  static calculate() {
    let values = this.getFormValues();
    let planets = [];
    switch (values.planet) {
      case "Select Planet":
        planets = [
          SpaceEngineers.Planet.Earth,
          SpaceEngineers.Planet.Moon,
          SpaceEngineers.Planet.Mars
        ];
        break;
      case "planetEarth":
        planets = [SpaceEngineers.Planet.Earth];
        break;
      default: // render nothing
    }
    var chart = new CanvasJS.Chart("thrust_chart", {
      axisX: { valueFormatString: "#,##0 km" },
      data: []
    });

    for(let planet of planets) {
      let data = {
        type: "spline",
        showInLegend: true,
        name: planet.name,
        dataPoints: []
      };
      for(let height_km = 0; height_km <= planet.gravityEnd_km; height_km++) {
        let thrust_kN = 0;
        thrust_kN += values.tAtmoSmall  * planet.getThruster_kN(height_km, "atmospheric", values.grid, "small");
        thrust_kN += values.tAtmoLarge  * planet.getThruster_kN(height_km, "atmospheric", values.grid, "large");
        thrust_kN += values.tHydroSmall * planet.getThruster_kN(height_km, "hydrogen",    values.grid, "small");
        thrust_kN += values.tHydroLarge * planet.getThruster_kN(height_km, "hydrogen",    values.grid, "large");
        thrust_kN += values.tIonSmall   * planet.getThruster_kN(height_km, "ion",         values.grid, "small");
        thrust_kN += values.tIonLarge   * planet.getThruster_kN(height_km, "ion",         values.grid, "large");
        data.dataPoints.push({ x: height_km, y: thrust_kN });
      }
      chart.options.data.push(data);
    }
    console.debug(chart.options);
    chart.render();
  } // calculate()

  static getFormValues() {
    let fields = {
      grid    : document.getElementById("thrust_size"),
      weight  : document.getElementById("thrust_weight"),
      planet  : document.getElementById("thrust_planet"),
      o2h2    : document.getElementById("thrust_o2h2Gens"),
      h2Small : document.getElementById("thrust_hydroTanksSmall"),
      h2Large : document.getElementById("thrust_hydroTanksLarge"),

      tAtmoSmall  : document.getElementById("thrust_thrustersAtmoSmall"),
      tAtmoLarge  : document.getElementById("thrust_thrustersAtmoLarge"),
      tHydroSmall : document.getElementById("thrust_thrustersHydroSmall"),
      tHydroLarge : document.getElementById("thrust_thrustersHydroLarge"),
      tIonSmall   : document.getElementById("thrust_thrustersIonSmall"),
      tIonLarge   : document.getElementById("thrust_thrustersIonLarge")
    };

    // collect these fields into values, as floats if possible
    let values = {};
    for(let key in fields) {
      let value = null;
      let field = fields[key];
      if(field instanceof HTMLSelectElement) {
        value = field.options[field.selectedIndex].value;
      } else if(field instanceof HTMLInputElement) {
        value = field.value;
      } else {
        throw "Cannot parse: " + field + "\nFrom key: " + key;
      }
      if(!isNaN(parseFloat(value))) {
        values[key] = parseFloat(value);
      } else if(value == "") {
        values[key] = 0;
      } else {
        values[key] = value;
      }
    }
    console.debug(values);
    return values;
  } // getFormValues()
} // class ThrustGUI
