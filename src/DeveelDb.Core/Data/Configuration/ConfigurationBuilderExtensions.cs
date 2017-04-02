﻿// 
//  Copyright 2010-2017 Deveel
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
//


using System;
using System.IO;

namespace Deveel.Data.Configuration {
	public static class ConfigurationBuilderExtensions {
		public static IConfigurationBuilder Add(this IConfigurationBuilder builder, IConfigurationSource source, IConfigurationFormatter formatter) {
			using (var stream = source.InputStream) {
				formatter.LoadInto(builder, stream);
			}

			return builder;
		}

		public static IConfigurationBuilder AddProperties(this IConfigurationBuilder builder, IConfigurationSource source)
			=> builder.Add(source, new PropertiesFormatter());

		public static IConfigurationBuilder AddPropertiesString(this IConfigurationBuilder builder, string source)
			=> builder.AddProperties(new StringConfigurationSource(source));

		public static IConfigurationBuilder AddPropertiesFile(this IConfigurationBuilder builder, string fileName)
			=> builder.AddProperties(new FileConfigurationSource(fileName));

		public static IConfigurationBuilder AddPropertiesStream(this IConfigurationBuilder builder, Stream stream)
			=> builder.AddProperties(new StreamConfigurationSource(stream));
	}
}