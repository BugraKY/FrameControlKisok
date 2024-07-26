#!/bin/bash
lsblk -o NAME,MODEL,VENDOR,TRAN | grep usb
