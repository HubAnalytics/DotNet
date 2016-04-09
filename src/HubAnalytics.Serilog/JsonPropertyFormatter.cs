using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Serilog.Events;
using IPropertyDictionary = System.Collections.Generic.IReadOnlyDictionary<string, Serilog.Events.LogEventPropertyValue>;
using IScalarDictionary = System.Collections.Generic.IReadOnlyDictionary<Serilog.Events.ScalarValue, Serilog.Events.LogEventPropertyValue>;

namespace HubAnalytics.Serilog
{
    // Based on the Serilog code - we don't want to serialize the whole log, just the properties
    internal class JsonPropertyFormatter
    {
        private readonly IDictionary<Type, Action<object, bool, TextWriter>> _literalWriters;

        public JsonPropertyFormatter()
        {
            _literalWriters = new Dictionary<Type, Action<object, bool, TextWriter>>
            {
                { typeof(bool), (v, q, w) => WriteBoolean((bool)v, w) },
                { typeof(char), (v, q, w) => WriteString(((char)v).ToString(), w) },
                { typeof(byte), WriteToString },
                { typeof(sbyte), WriteToString },
                { typeof(short), WriteToString },
                { typeof(ushort), WriteToString },
                { typeof(int), WriteToString },
                { typeof(uint), WriteToString },
                { typeof(long), WriteToString },
                { typeof(ulong), WriteToString },
                { typeof(float), WriteToString },
                { typeof(double), WriteToString },
                { typeof(decimal), WriteToString },
                { typeof(string), (v, q, w) => WriteString((string)v, w) },
                { typeof(DateTime), (v, q, w) => WriteDateTime((DateTime)v, w) },
                { typeof(DateTimeOffset), (v, q, w) => WriteOffset((DateTimeOffset)v, w) },
                { typeof(ScalarValue), (v, q, w) => WriteLiteral(((ScalarValue)v).Value, w, q) },
                { typeof(SequenceValue), (v, q, w) => WriteSequence(((SequenceValue)v).Elements, w) },
                { typeof(DictionaryValue), (v, q, w) => WriteDictionary(((DictionaryValue)v).Elements, w) },
                { typeof(StructureValue), (v, q, w) => WriteStructure(((StructureValue)v).TypeTag, ((StructureValue)v).Properties, w) },
            };
        }

        /// <summary>
        /// Writes out the attached properties
        /// </summary>
        public void WriteProperties(IPropertyDictionary properties, TextWriter output)
        {
            output.Write("{");
            WritePropertiesValues(properties, output);
            output.Write("}");
        }

        /// <summary>
        /// Writes out the attached properties values
        /// </summary>
        protected virtual void WritePropertiesValues(IPropertyDictionary properties, TextWriter output)
        {
            var precedingDelimiter = "";
            foreach (var property in properties)
            {
                WriteJsonProperty(property.Key, property.Value, ref precedingDelimiter, output);
            }
        }

        /// <summary>
        /// Writes out a structure property
        /// </summary>
        protected virtual void WriteStructure(string typeTag, IEnumerable<LogEventProperty> properties, TextWriter output)
        {
            output.Write("{");

            var delim = "";
            if (typeTag != null)
                WriteJsonProperty("_typeTag", typeTag, ref delim, output);

            foreach (var property in properties)
                WriteJsonProperty(property.Name, property.Value, ref delim, output);

            output.Write("}");
        }

        /// <summary>
        /// Writes out a sequence property
        /// </summary>
        protected virtual void WriteSequence(IEnumerable elements, TextWriter output)
        {
            output.Write("[");
            var delim = "";
            foreach (var value in elements)
            {
                output.Write(delim);
                delim = ",";
                WriteLiteral(value, output);
            }
            output.Write("]");
        }

        /// <summary>
        /// Writes out a dictionary 
        /// </summary>
        protected virtual void WriteDictionary(IScalarDictionary elements, TextWriter output)
        {
            output.Write("{");
            var delim = "";
            foreach (var e in elements)
            {
                output.Write(delim);
                delim = ",";
                WriteLiteral(e.Key, output, true);
                output.Write(":");
                WriteLiteral(e.Value, output);
            }
            output.Write("}");
        }

        /// <summary>
        /// Writes out a json property with the specified value on output writer
        /// </summary>
        protected virtual void WriteJsonProperty(string name, object value, ref string precedingDelimiter, TextWriter output)
        {
            output.Write(precedingDelimiter);
            output.Write("\"");
            output.Write(name);
            output.Write("\":");
            WriteLiteral(value, output);
            precedingDelimiter = ",";
        }

        /// <summary>
        /// Allows a subclass to write out objects that have no configured literal writer.
        /// </summary>
        /// <param name="value">The value to be written as a json construct</param>
        /// <param name="output">The writer to write on</param>
        protected virtual void WriteLiteralValue(object value, TextWriter output)
        {
            WriteString(value.ToString(), output);
        }

        void WriteLiteral(object value, TextWriter output, bool forceQuotation = false)
        {
            if (value == null)
            {
                output.Write("null");
                return;
            }

            Action<object, bool, TextWriter> writer;
            if (_literalWriters.TryGetValue(value.GetType(), out writer))
            {
                writer(value, forceQuotation, output);
                return;
            }

            WriteLiteralValue(value, output);
        }

        static void WriteToString(object number, bool quote, TextWriter output)
        {
            if (quote) output.Write('"');

            var fmt = number as IFormattable;
            if (fmt != null)
                output.Write(fmt.ToString(null, CultureInfo.InvariantCulture));
            else
                output.Write(number.ToString());

            if (quote) output.Write('"');
        }

        static void WriteBoolean(bool value, TextWriter output)
        {
            output.Write(value ? "true" : "false");
        }

        static void WriteOffset(DateTimeOffset value, TextWriter output)
        {
            output.Write("\"");
            output.Write(value.ToString("o"));
            output.Write("\"");
        }

        static void WriteDateTime(DateTime value, TextWriter output)
        {
            output.Write("\"");
            output.Write(value.ToString("o"));
            output.Write("\"");
        }

        static void WriteString(string value, TextWriter output)
        {
            var content = Escape(value);
            output.Write("\"");
            output.Write(content);
            output.Write("\"");
        }

        /// <summary>
        /// Perform simple JSON string escaping on <paramref name="s"/>.
        /// </summary>
        /// <param name="s">A raw string.</param>
        /// <returns>A JSON-escaped version of <paramref name="s"/>.</returns>
        public static string Escape(string s)
        {
            if (s == null) return null;

            StringBuilder escapedResult = null;
            var cleanSegmentStart = 0;
            for (var i = 0; i < s.Length; ++i)
            {
                var c = s[i];
                if (c < (char)32 || c == '\\' || c == '"')
                {

                    if (escapedResult == null)
                        escapedResult = new StringBuilder();

                    escapedResult.Append(s.Substring(cleanSegmentStart, i - cleanSegmentStart));
                    cleanSegmentStart = i + 1;

                    switch (c)
                    {
                        case '"':
                            {
                                escapedResult.Append("\\\"");
                                break;
                            }
                        case '\\':
                            {
                                escapedResult.Append("\\\\");
                                break;
                            }
                        case '\n':
                            {
                                escapedResult.Append("\\n");
                                break;
                            }
                        case '\r':
                            {
                                escapedResult.Append("\\r");
                                break;
                            }
                        case '\f':
                            {
                                escapedResult.Append("\\f");
                                break;
                            }
                        case '\t':
                            {
                                escapedResult.Append("\\t");
                                break;
                            }
                        default:
                            {
                                escapedResult.Append("\\u");
                                escapedResult.Append(((int)c).ToString("X4"));
                                break;
                            }
                    }
                }
            }

            if (escapedResult != null)
            {
                if (cleanSegmentStart != s.Length)
                    escapedResult.Append(s.Substring(cleanSegmentStart));

                return escapedResult.ToString();
            }

            return s;
        }
    }
}
