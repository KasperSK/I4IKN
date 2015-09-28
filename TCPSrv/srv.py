#!/usr/bin/env python
# -*- coding: utf-8 -*-

import socket
import logging
from os.path import basename, isfile, getsize
import hashlib
import re

base = "/srvdir/"

logging.basicConfig(format='%(levelname)s:%(message)s', level=logging.DEBUG)

def hashfile(afile, hasher, blocksize=65536):
	buf = afile.read(blocksize)
	while len(buf) > 0:
		hasher.update(buf)
		buf = afile.read(blocksize)
	return hasher.hexdigest()

def sha1(fname):
	with open(fname, 'rb') as f:
		return hashfile(f, hashlib.sha1())

class MessageFrame:

	Delimiter = "\r\n\r\n"
	
	@staticmethod
	def getFrameFromSocket(socket):
		frame = MessageFrame()
		while not frame.isvalid:
			frame.add(socket.recv(1))
		return frame
		
	def __init__(self):
		self.buffer = bytearray()

	@property
	def data(self):
		return self.buffer[:-len(self.Delimiter)]
	
	@property
	def isvalid(self):
		if len(self.buffer) < len(self.Delimiter):
			return False
		else:
			return self.buffer[-len(self.Delimiter):] == self.Delimiter
	
	def add(self, ch):
		self.buffer.extend(ch)
	
class Message:
	
	ValidMessageRegex = re.compile(r"([A-Za-z]+)\ (.+)\ FS/([0-9\.]+)")

	def __init__(self, frame):
		(self.command, self.data, self.version) = self.ParseFrame(frame.data)
	
	@staticmethod
	def ParseFrame(frame):
		match = re.search(Message.ValidMessageRegex, frame)
		if match:
			logging.info("Valid Message received")
			return (str(match.group(1)), str(match.group(2)), str(match.group(3)))
		else:
			logging.info("Invalid Message received")
			raise ValueError('Invalid Message')
			
	def __str__(self):
		return "============================\nCMD: " + self.command + "\n" + "DATA: " + self.data + "\n" +"VER: " + self.version + "\n============================"
		

class FSProtocol:

	def __init__(self, client):
		self.client = client
	
	def getMessage(self):
		while 1:
			try:
				return Message(MessageFrame.getFrameFromSocket(self.client))
			except ValueError:
				self.client.send("FS/1.0 400 Bad Request\r\n\r\n")
		
	def run(self):
		while 1:
			msg = self.getMessage()
			logging.debug("\n"+ str(msg))			
			if msg.command == "GET":
				logging.info("GET Command detected")
				srcfile = base + msg.data
				srcfile = re.sub("//","/",srcfile)
				srcfile = re.sub("//","/",srcfile)
				srcfile = re.sub("/\.\./","/",srcfile)
				
				if not isfile(srcfile):
					logging.info("%s is not a file", srcfile)
					self.client.send("FS/1.0 404 File Not Found\r\n\r\n")
					continue
				logging.info("%s is a file", srcfile)
				self.client.send("FS/1.0 200 OK\r\n")
				
				shavalue = sha1(srcfile)
				logging.info("Sha1: %s", shavalue)
				self.client.send("Sha1: " + shavalue + "\r\n")
				
				sizevalue = getsize(srcfile)
				logging.info("Content-Length: %s", sizevalue)
				self.client.send("Content-Length: " + str(sizevalue) + "\r\n\r\n")
				
				with open(srcfile,'rb') as f:
					buffer = f.read(1000)
					while len(buffer) > 0:
						self.client.send(buffer)
						buffer = f.read(1000)
						
				
				
			elif msg.command == "QUIT":
				logging.info("QUIT Command detected")
				logging.info("Shutting down")
				self.client.shutdown(socket.SHUT_RDWR)
				return
			else:
				logging.info("%s is not a valid command", msg.command)
				self.client.send("FS/1.0 400 Bad Request\r\n\r\n")


class Srv(object):
	def __init__(self):
		self.socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
		self.socket.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
		self.socket.bind(('', 9000))
		self.socket.listen(5)

	def stop(self):
		self.socket.shutdown(socket.SHUT_RDWR)

	def getclient(self):
		(clientsocket, address) = self.socket.accept()
		return clientsocket

if __name__ == "__main__":
	server = Srv()
	while 1:
		c = server.getclient()
		f = FSProtocol(c)
		f.run()
	server.stop()
	c.shutdown(1)
