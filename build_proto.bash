cd ProtoChatSimple.Domain

protoc --plugin=protoc-gen-grpc=/Users/korobkoroman/grpc/cmake/build/grpc_csharp_plugin \
       --grpc_out=obj/Debug/net9.0/Protos \
       --csharp_out=obj/Debug/net9.0/Protos \
       -I Protos \
       Chat.proto greet.proto

