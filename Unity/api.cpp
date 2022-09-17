#include <libfreenect2/libfreenect2.hpp>
#include <libfreenect2/frame_listener_impl.h>
#include <libfreenect2/registration.h>
#include <libfreenect2/packet_pipeline.h>
#include <libfreenect2/logger.h>
#include <iostream>
#include <fstream>
#include <cstring>
using namespace std;

int main() {
    // GET DEVICE
    libfreenect2::Freenect2 freenect2;
    libfreenect2::Freenect2Device* dev = 0;
    libfreenect2::PacketPipeline* pipeline = 0;
    std::string serial;
    if(freenect2.enumerateDevices() == 0) {
        std::cout << "no device connected!" << std::endl;
        return -1;
    }
        serial = freenect2.getDefaultDeviceSerialNumber();
    std::cout << "Serial Number: " << serial << std::endl;
    int types = libfreenect2::Frame::Depth;
    libfreenect2::SyncMultiFrameListener listener(types);
    libfreenect2::FrameMap frames;
        pipeline = new libfreenect2::CpuPacketPipeline();
    dev = freenect2.openDevice(serial, pipeline);
    dev->setColorFrameListener(&listener);
    dev->setIrAndDepthFrameListener(&listener);
    if(!dev->startStreams(true, true)) {
        std::cout << "Failed to start stream" << std::endl;
        return -1;
    }
    libfreenect2::Registration* registration = new libfreenect2::Registration(dev->getIrCameraParams(), dev->getColorCameraParams());
    libfreenect2::Frame undistorted(512, 424, 4), registered(512, 424, 4);
    //Log info
    std::cout << "device serial: " << dev->getSerialNumber() << std::endl;
    std::cout << "device firmware: " << dev->getFirmwareVersion() << std::endl;
    //Fetch loop
    while(true) {
        //Timeout error
        if(!listener.waitForNewFrame(frames, 10 * 1000)) {
            std::cout << "timeout!" << std::endl;
            return -1;
        }
cout << "Frame written 1" << endl;
        //Get frames
        libfreenect2::Frame* depth = frames[libfreenect2::Frame::Depth];
        libfreenect2::Frame* rgb = frames[libfreenect2::Frame::Color];
registration->apply(rgb, depth, &undistorted, &registered);
        //Write depth data to a file
        std::ofstream file;
        file.open("depthdata.bin", std::ios::out|std::ios::binary);
        file.write((char*)depth->data,512*424*4);
        file.flush();
        //Clean up
        file.close();
        listener.release(frames);
        cout << "Frame written 2" << endl;
    }
    //Close device on exit
    dev->stop();
    dev->close();
    return 0;
}
