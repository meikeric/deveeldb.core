﻿using System;
using System.IO;

namespace Deveel.Data.Configuration {
	/// <summary>
	/// Provides the format to load and store configurations
	/// in and from a stream.
	/// </summary>
	/// <remarks>
	/// Implementations of this interface will apply a specific
	/// format to the data stored and retrieved (for example
	/// an XML fragmanet, a key/value file, a binary file, etc.).
	/// </remarks>
	public interface IConfigurationFormatter {
		/// <summary>
		/// Loads a stored configuration from the given stream
		/// into the configuration argument.
		/// </summary>
		/// <param name="config">The configuration object inside of which
		/// to load the configurations from the the given stream.</param>
		/// <param name="inputStream">The stream from where to read the
		/// configurations formatted into the object provided as argument.</param>
		/// <exception cref="ArgumentException">
		/// If the provided <paramref name="inputStream"/> cannot be read.
		/// </exception>
		/// <exception cref="ArgumentNullException">
		/// If either <paramref name="config"/> or <paramref name="inputStream"/>
		/// are <c>null</c>.
		/// </exception>
		void LoadInto(IConfiguration config, Stream inputStream);

		/// <summary>
		/// Stores the given level of configurations into the output stream
		/// provided, in the format handled by this interface.
		/// </summary>
		/// <param name="config">The source of the configurations to store.</param>
		/// <param name="level">The level of the configurations to load from
		/// the source and store.</param>
		/// <param name="outputStream">The destination stream where the formatter
		/// saves the configurations retrieved from the source.</param>
		/// <seealso cref="ConfigurationLevel"/>
		/// <exception cref="ArgumentException">
		/// If the provided <paramref name="outputStream"/> cannot be written.
		/// </exception>
		/// <exception cref="ArgumentNullException">
		/// If either <paramref name="config"/> or <paramref name="outputStream"/>
		/// are <c>null</c>.
		/// </exception>
		void SaveFrom(IConfiguration config, ConfigurationLevel level, Stream outputStream);
	}
}