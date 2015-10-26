#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <arpa/inet.h>
#include <netinet/in.h>
#include <sys/types.h>
#include <sys/socket.h>
#include <netdb.h>
#include <unistd.h>

#define BUFFER_SIZE 1024
#define PORT 9999

int main(int argc, char *argv[]) {
    struct sockaddr_in udpserv;
    int len = sizeof(struct sockaddr_in);
    char buf[BUFFER_SIZE];
    struct hostent *host;
    int n, s;

    s = socket(AF_INET, SOCK_DGRAM, IPPROTO_UDP)
	
    memset((char *) &udpserv, 0, len);
    udpserv.sin_family = AF_INET;
    udpserv.sin_port = htons(PORT);
    udpserv.sin_addr = *((struct in_addr*) host->h_addr);
    sendto(s, argv[3], strlen(argv[3]), 0, (struct sockaddr *) &udpserv, len)
	
    while ((n = recvfrom(s, buf, BUFFER_SIZE, 0, (struct sockaddr *) &udpserv, &len)) != -1) {
		printf("Modtaget %s:%d: ", inet_ntoa(udpserv.sin_addr), ntohs(udpserv.sin_port)); 
		fflush(stdout);
		write(1, buf, n);
		write(1, "\n", 1);
    }

    close(s);
    return 0;
}
