﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flash411
{
    /// <summary>
    /// This class is just here to enable testing without any actual interface hardware.
    /// </summary>
    /// <remarks>
    /// Eventually the Receive method should return simulated VPW responses.
    /// </remarks>
    class MockAvt852 : IPort
    {
        public const string PortName = "Mock AVT 852";

        private byte[] responseBuffer;

        private int sentSoFar;

        private ILogger logger;
        
        public MockAvt852(ILogger logger)
        {
            // this.pcm = new MockPcm(logger);
            this.logger = logger;
        }

        /// <summary>
        /// This returns the string that appears in the drop-down list.
        /// </summary>
        public override string ToString()
        {
            return PortName;
        }

        /// <summary>
        /// Pretend to open a port.
        /// </summary>
        Task IPort.OpenAsync(PortConfiguration configuration)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Pretend to close a port.
        /// </summary>
        public void Dispose()
        {
        }

        /// <summary>
        /// Send bytes to the mock PCM.
        /// </summary>
        Task IPort.Send(byte[] buffer)
        {
            this.logger.AddDebugMessage("MockAvt852 received: " + buffer.ToHex());

            if (Utility.CompareArrays(buffer, Avt852Device.AVT_RESET))
            {
                responseBuffer = Avt852Device.AVT_852_IDLE;
            }
            else if(Utility.CompareArrays(buffer, Avt852Device.AVT_REQUEST_MODEL))
            {
                responseBuffer = new byte[] { 0x93, 0x28, 0x08, 0x52 };
            }
            else if(Utility.CompareArrays(buffer, Avt852Device.AVT_REQUEST_FIRMWARE))
            {
                responseBuffer = new byte[] { 0x92, 0x04, 0x15 };
            }
            else if(Utility.CompareArrays(buffer, Avt852Device.AVT_ENTER_VPW_MODE))
            {
                responseBuffer = Avt852Device.AVT_VPW;
            }

            this.sentSoFar = 0;

            return Task.CompletedTask;
        }

        /// <summary>
        /// Receive bytes from the mock PCM.
        /// </summary>
        Task<int> IPort.Receive(byte[] buffer, int offset, int count)
        {
            int sent = 0;
            for (int index = 0; index < count; index++)
            {
                buffer[offset+index] = this.responseBuffer[this.sentSoFar];
                this.sentSoFar++;
                sent++;
            }
                        
            this.logger.AddDebugMessage("MockAvt852 sending: " + this.responseBuffer.ToHex());

            return Task.FromResult(sent);
        }

        /// <summary>
        /// Discard anything in the input and output buffers.
        /// </summary>
        public void DiscardBuffers()
        {
        }
    }
}
