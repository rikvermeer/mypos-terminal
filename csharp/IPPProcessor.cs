using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace myPOS
{
	internal class IPPProcessor
	{
		public class Field
		{
			public string name;

			public string str_data;

			public int bin_data_size;

			public byte[] bin_data;

			public Field()
			{
			}

			public Field(string name, string data)
			{
				this.name = name;
				str_data = data;
			}

			public Field(string name, byte[] data)
			{
				this.name = name;
				bin_data = data;
				bin_data_size = data.Length;
			}
		}

		public List<Field> fields;

		private IPPProcessor()
		{
			fields = new List<Field>();
		}

		public bool IsValid()
		{
			int result = 0;
			Field field = Get("PROTOCOL");
			if (field == null || field.str_data != "IPP")
			{
				return false;
			}
			field = Get("METHOD");
			if (field == null || field.str_data == "")
			{
				return false;
			}
			field = Get("SID");
			if (field == null || field.str_data == "")
			{
				return false;
			}
			field = Get("VERSION");
			if (field == null)
			{
				return false;
			}
			if (!int.TryParse(field.str_data, out result))
			{
				return false;
			}
			if (result < 200)
			{
				return false;
			}
			return true;
		}

		public IPPProcessor CreateResult(int status)
		{
			IPPProcessor iPPProcessor = new IPPProcessor();
			iPPProcessor.Add(Get("PROTOCOL"));
			iPPProcessor.Add(Get("VERSION"));
			iPPProcessor.Add(Get("METHOD"));
			iPPProcessor.Add(Get("SID"));
			iPPProcessor.Add("STATUS", status.ToString());
			iPPProcessor.Add(Get("STAGE"));
			return iPPProcessor;
		}

		public static IPPProcessor CreateRequest(string method)
		{
			return new IPPProcessor
			{
				fields = 
				{
					new Field("PROTOCOL", "IPP"),
					new Field("VERSION", "200"),
					new Field("METHOD", method),
					new Field("SID", Guid.NewGuid().ToString())
				}
			};
		}

		public byte[] GetDataForSending()
		{
			List<byte> list = new List<byte>();
			list.Add(0);
			list.Add(0);
			for (int i = 0; i < fields.Count; i++)
			{
				if (fields[i] != null)
				{
					switch (fields[i].name)
					{
					case "PRIMARY_CHAIN":
					case "SECONDARY_CHAIN":
					case "FINGERPRINT":
					case "CHAIN":
					case "PRINT_DATA":
						list.AddRange(Encoding.ASCII.GetBytes(fields[i].name));
						list.Add(61);
						list.AddRange(fields[i].bin_data);
						list.Add(13);
						list.Add(10);
						break;
					case "DATA":
						list.AddRange(Encoding.ASCII.GetBytes(fields[i].name));
						list.Add(61);
						list.Add((byte)(fields[i].bin_data_size / 256));
						list.Add((byte)(fields[i].bin_data_size % 256));
						list.AddRange(fields[i].bin_data);
						list.Add(13);
						list.Add(10);
						break;
					default:
						list.AddRange(Encoding.ASCII.GetBytes(fields[i].name));
						list.Add(61);
						list.AddRange(Encoding.ASCII.GetBytes(fields[i].str_data));
						list.Add(13);
						list.Add(10);
						break;
					}
				}
			}
			list[0] = (byte)(list.Count / 256);
			list[1] = (byte)(list.Count % 256);
			return list.ToArray();
		}

		public static bool TryParse(byte[] data, out IPPProcessor output)
		{
			output = null;
			IPPProcessor iPPProcessor = new IPPProcessor();
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			while (num < data.Length)
			{
				Field field = new Field();
				num2 = num;
				for (num++; num < data.Length && data[num] != 61; num++)
				{
				}
				if (num >= data.Length)
				{
					return false;
				}
				field.name = Encoding.ASCII.GetString(data, num2, num - num2);
				num++;
				switch (field.name)
				{
				case "DATA":
				case "CERT":
					if (num + 2 >= data.Length)
					{
						return false;
					}
					num3 = (data[num] << 8) + data[num + 1];
					num2 = num;
					if (num + num3 > data.Length)
					{
						return false;
					}
					num += num3;
					if (data[num] != 13 || data[num + 1] != 10)
					{
						return false;
					}
					num += 2;
					field.bin_data_size = num3;
					field.bin_data = new byte[num3];
					Array.Copy(data, num2, field.bin_data, 0, num3);
					break;
				case "PRIMARY_CHAIN":
				case "SECONDARY_CHAIN":
				case "CHAIN":
					num2 = num;
					while (data[num] != 13 || data[num + 1] != 10)
					{
						num += 20;
						if (num >= data.Length)
						{
							return false;
						}
						if (data[num] != 59)
						{
							return false;
						}
						num++;
						if (num >= data.Length)
						{
							break;
						}
					}
					if (num >= data.Length)
					{
						return false;
					}
					num3 = num - num2;
					num += 2;
					if (num3 > 0)
					{
						field.bin_data_size = num3;
						field.bin_data = new byte[num3];
						Array.Copy(data, num2, field.bin_data, 0, num3);
					}
					break;
				case "FINGERPRINT":
					num2 = num;
					num += 20;
					if (num >= data.Length)
					{
						return false;
					}
					if (data[num] != 13 || data[num + 1] != 10)
					{
						return false;
					}
					num3 = num - num2;
					num += 2;
					field.bin_data_size = num3;
					field.bin_data = new byte[num3];
					Array.Copy(data, num2, field.bin_data, 0, num3);
					break;
				default:
					num2 = num;
					for (; num < data.Length - 1 && (data[num] != 13 || data[num + 1] != 10); num++)
					{
					}
					if (num >= data.Length - 1)
					{
						return false;
					}
					field.str_data = Encoding.ASCII.GetString(data, num2, num - num2);
					num += 2;
					break;
				}
				iPPProcessor.fields.Add(field);
			}
			output = iPPProcessor;
			return true;
		}

		public void Add(string ParamName, string ParamData)
		{
			if (!string.IsNullOrEmpty(ParamData))
			{
				fields.Add(new Field(ParamName, ParamData));
			}
		}

		public void Add(string ParamName, byte[] ParamData)
		{
			if (ParamData != null && ParamData.Length != 0)
			{
				fields.Add(new Field(ParamName, ParamData));
			}
		}

		public void Add(Field param)
		{
			fields.Add(param);
		}

		public Field Get(string ParamName)
		{
			return fields.FirstOrDefault((Field x) => x.name == ParamName);
		}
	}
}
