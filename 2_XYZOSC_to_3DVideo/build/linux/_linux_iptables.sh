#!/bin/sh
sudo iptables -I INPUT -p udp -j ACCEPT
