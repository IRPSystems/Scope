
using DeviceCommunicators.MCU;
using DeviceCommunicators.Models;
using Entities.Models;
using MCUScope.Enums;
using MCUScope.Models;
using System;

namespace MCUScope.Services
{
	public class BuildRequestMessagesService
	{
		private const uint _func1ID = 0xE4DF2A;
		private const uint _func2ID = 0x0F8026;
		private const uint _func3ID = 0x1063FF;
		private const uint _func4ID = 0x6BDB27;


		private const uint _funcForceTrig = 0xBD595B;


		public byte[] BuildMessage1(
			int numOfParams,
			int recordGap,
			bool isContiuous,
			TriggerPositionTypesEnum triggerPosition)
		{
			byte[] data = new byte[8];

			Array.Copy(
				GetBytesOfID(_func1ID), data, 3);

			data[4] = (byte)numOfParams;
			data[5] = (byte)recordGap;

			if (isContiuous)
				data[6] = 1;
			else
				data[6] = 0;

			data[7] = (byte)triggerPosition;

			//data[4] = 1;
			//data[5] = 1;

			////if (isContiuous)
			////	data[6] = 1;
			////else
			//	data[6] = 0;

			//data[7] = 1;

			return data;
		}




		public byte[] BuildMessage2(SelectedParameterData parameter)
		{
			byte[] data = new byte[8];

			Array.Copy(
				GetBytesOfID(_func2ID), data, 3);

			if(!(parameter.Parameter is MCU_ParamData mcuParam))
				return null;

			byte[] id = new byte[3];
			mcuParam.GetMessageID(ref id);
			Array.Copy(id, 0, data, 5, 3);

			
			data[4] = parameter.Scale;

			return data;
		}



		public byte[] BuildMessage3(
			DeviceParameterData triggerKeyword,
			TriggerTypesEnum triggerType)
		{
			byte[] data = new byte[8];

			Array.Copy(
				GetBytesOfID(_func3ID), data, 3);

			if(triggerKeyword == null)
			{
				data[7] = (byte)triggerType;
				return data;
			}

			if (!(triggerKeyword is MCU_ParamData mcuParam))
				return null;

			data[4] = (byte)triggerType;


			byte[] id = new byte[3];
			mcuParam.GetMessageID(ref id);
			Array.Copy(id, 0, data, 5, 3);

			

			//data[4] = 0x48;
			//data[5] = 0x90;
			//data[6] = 0xED;

			return data;
		}


		public byte[] BuildMessage4(
			int triggerThreshold)
		{
			byte[] data = new byte[8];

			Array.Copy(
				GetBytesOfID(_func4ID), data, 3);




			//byte[] data1 = new byte[4];
			//for (int i = 0; i < data1.Length; i++)
			//{
			//	data1[i] = (byte)(triggerThreshold >> (8 * i));
			//}
			//Array.Copy(data1, 0, data, 4, 4);

			//data[6] = 0x3;
			//data[7] = 0xE8;

			return data;
		}

		public byte[] BuildForceTriggerMessage()
		{
			
			byte[] data = new byte[8];

			Array.Copy(GetBytesOfID(_funcForceTrig), data, 3);

			return data;
		}

		private byte[] GetBytesOfID(uint val)
		{
			byte[] data1 = new byte[3];
			data1[0] = (byte)(val >> (8 * 2));
			data1[1] = (byte)(val >> (8 * 1));
			data1[2] = (byte)(val >> (8 * 0));


			return data1;
		}
	}
}
