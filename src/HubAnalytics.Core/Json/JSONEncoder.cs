﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using HubAnalytics.Core.Model;
using Environment = HubAnalytics.Core.Model.Environment;

// ReSharper disable once CheckNamespace
namespace SimpleJSON
{
    // ReSharper disable once InconsistentNaming
    internal class JSONEncoder
    {
        public static string Encode(object obj)
        {
            var encoder = new JSONEncoder();
            encoder.EncodeObject(obj);
            return encoder._buffer.ToString();
        }

        private readonly StringBuilder _buffer = new StringBuilder();

        internal static readonly Dictionary<char, string> EscapeChars =
            new Dictionary<char, string>
                {
                    { '"', "\\\"" },
                    { '\\', "\\\\" },
                    { '\b', "\\b" },
                    { '\f', "\\f" },
                    { '\n', "\\n" },
                    { '\r', "\\r" },
                    { '\t', "\\t" },
                    { '\u2028', "\\u2028" },
                    { '\u2029', "\\u2029" }
                };

        private JSONEncoder() { }

        private IDictionary ToDictionary(Event ev)
        {
            return new Dictionary<string, object>
            {
                { "EventEndDateTime", ev.EventEndDateTime },
                { "CorrelationDepths", ev.CorrelationDepths },
                { "CorrelationIds", ev.CorrelationIds },
                { "Data", ev.Data },
                { "EventStartDateTime", ev.EventStartDateTime },
                { "EventType", ev.EventType },
                { "SessionId", ev.SessionId },
                { "UserId", ev.UserId }
            };
        }

        private IDictionary ToDictionary(EventBatch evb)
        {
            return new Dictionary<string, object>
            {
                {"ApplicationVersion", evb.ApplicationVersion},
                {"Environment", evb.Environment},
                {"Events", evb.Events},
                {"ReceivedAt", evb.ReceivedAt},
                {"Source", evb.Source}
            };
        }

        private IDictionary ToDictionary(Environment env)
        {
            return new Dictionary<string, object>
            {
                { "AvailablePhysicalMemory", env.AvailablePhysicalMemory },
                { "Locale", env.Locale },
                { "MachineName", env.MachineName },
                { "OperatingSystemVerson", env.OperatingSystemVerson },
                { "ProcessorCount", env.ProcessorCount },
                { "TotalPhysicalMemory", env.TotalPhysicalMemory },
                { "UserAgentString", env.UserAgentString },
                { "UtcOffset", env.UtcOffset }
            };
        }

        private IDictionary ToDictionary(StackTraceEntry entry)
        {
            return new Dictionary<string, object>
            {
                {"Assembly", entry.Assembly},
                {"Class", entry.Class},
                {"Column", entry.Column},
                {"Filename", entry.Filename},
                {"Line", entry.Line},
                {"Method", entry.Method}
            };
        }

        private void EncodeObject(object obj)
        {
            if (obj == null)
            {
                EncodeNull();
            }
            else if (obj is string)
            {
                EncodeString((string)obj);
            }
            else if (obj is float)
            {
                EncodeFloat((float)obj);
            }
            else if (obj is double)
            {
                EncodeDouble((double)obj);
            }
            else if (obj is int)
            {
                EncodeLong((int)obj);
            }
            else if (obj is uint)
            {
                EncodeULong((uint)obj);
            }
            else if (obj is long)
            {
                EncodeLong((long)obj);
            }
            else if (obj is ulong)
            {
                EncodeULong((ulong)obj);
            }
            else if (obj is short)
            {
                EncodeLong((short)obj);
            }
            else if (obj is ushort)
            {
                EncodeULong((ushort)obj);
            }
            else if (obj is byte)
            {
                EncodeULong((byte)obj);
            }
            else if (obj is bool)
            {
                EncodeBool((bool)obj);
            }
            else if (obj is DateTimeOffset)
            {
                EncodeDateTimeOffset((DateTimeOffset) obj);
            }
            else if (obj is DateTime)
            {
                EncodeDateTime((DateTime)obj);
            }
            else if (obj is IDictionary)
            {
                EncodeDictionary((IDictionary)obj);
            }
            else if (obj is IEnumerable)
            {
                EncodeEnumerable((IEnumerable)obj);
            }
            else if (obj is Enum)
            {
                EncodeObject(Convert.ChangeType(obj, Enum.GetUnderlyingType(obj.GetType())));
            }
            else if (obj is Event)
            {
                EncodeDictionary(ToDictionary((Event) obj));
            }
            else if (obj is EventBatch)
            {
                EncodeDictionary(ToDictionary((EventBatch)obj));
            }
            else if (obj is Environment)
            {
                EncodeDictionary(ToDictionary((Environment)obj));
            }
            else if (obj is StackTraceEntry)
            {
                EncodeDictionary(ToDictionary((StackTraceEntry)obj));
            }
            else if (obj is JObject)
            {
                var jobj = (JObject)obj;
                switch (jobj.Kind)
                {
                    case JObjectKind.Array:
                        EncodeEnumerable(jobj.ArrayValue);
                        break;
                    case JObjectKind.Boolean:
                        EncodeBool(jobj.BooleanValue);
                        break;
                    case JObjectKind.Null:
                        EncodeNull();
                        break;
                    case JObjectKind.Number:
                        if (jobj.IsFractional)
                        {
                            EncodeDouble(jobj.DoubleValue);
                        }
                        else if (jobj.IsNegative)
                        {
                            EncodeLong(jobj.LongValue);
                        }
                        else
                        {
                            EncodeULong(jobj.ULongValue);
                        }
                        break;
                    case JObjectKind.Object:
                        EncodeDictionary(jobj.ObjectValue);
                        break;
                    case JObjectKind.String:
                        EncodeString(jobj.StringValue);
                        break;
                    default:
                        throw new ArgumentException("Can't serialize object of type " + obj.GetType().Name, "obj");
                }
            }
            else
            {
                throw new ArgumentException("Can't serialize object of type " + obj.GetType().Name, nameof(obj));
            }
        }

        private void EncodeNull()
        {
            _buffer.Append("null");
        }

        private void EncodeString(string str)
        {
            _buffer.Append('"');
            foreach (var c in str)
            {
                if (EscapeChars.ContainsKey(c))
                {
                    _buffer.Append(EscapeChars[c]);
                }
                else
                {
                    if (c > 0x80 || c < 0x20)
                    {
                        _buffer.Append("\\u" + Convert.ToString(c, 16).ToUpper().PadLeft(4, '0'));
                    }
                    else
                    {
                        _buffer.Append(c);
                    }
                }
            }
            _buffer.Append('"');
        }

        private void EncodeFloat(float f)
        {
            _buffer.Append(f.ToString(CultureInfo.InvariantCulture));
        }

        private void EncodeDouble(double d)
        {
            _buffer.Append(d.ToString(CultureInfo.InvariantCulture));
        }

        private void EncodeLong(long l)
        {
            _buffer.Append(l.ToString(CultureInfo.InvariantCulture));
        }

        private void EncodeULong(ulong l)
        {
            _buffer.Append(l.ToString(CultureInfo.InvariantCulture));
        }

        private void EncodeBool(bool b)
        {
            _buffer.Append(b ? "true" : "false");
        }

        private void EncodeDateTimeOffset(DateTimeOffset dto)
        {
            _buffer.Append('"');
            _buffer.Append(dto.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss.FFFFFFFK"));
            _buffer.Append('"');
        }

        private void EncodeDateTime(DateTime dt)
        {
            DateTimeOffset dto = dt.ToUniversalTime();
            _buffer.Append('"');
            _buffer.Append(dto.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss.FFFFFFFK"));
            _buffer.Append('"');
        }

        private void EncodeDictionary(IDictionary d)
        {
            var isFirst = true;
            _buffer.Append('{');
            foreach (DictionaryEntry pair in d)
            {
                if (!(pair.Key is string))
                {
                    throw new ArgumentException("Dictionary keys must be strings", "d");
                }
                if (!isFirst) _buffer.Append(',');
                EncodeString((string)pair.Key);
                _buffer.Append(':');
                EncodeObject(pair.Value);
                isFirst = false;
            }
            _buffer.Append('}');
        }

        private void EncodeEnumerable(IEnumerable e)
        {
            var isFirst = true;
            _buffer.Append('[');
            foreach (var obj in e)
            {
                if (!isFirst) _buffer.Append(',');
                EncodeObject(obj);
                isFirst = false;
            }
            _buffer.Append(']');
        }
    }
}

