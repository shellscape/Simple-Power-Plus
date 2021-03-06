﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SimplePowerPlus {

	[DataContract(Name = "config")]
	public class Config : Shellscape.Configuration.Config<Config> {

		[DataMember(Name = "ask")]
		public Boolean Ask { get; set; }

		protected override string ApplicationName {
			get { return "Simple Power Plus"; }
		}

		protected override void SetDefaults() {
			this.Ask = true;
		}

	}
}
