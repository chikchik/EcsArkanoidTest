package com.dm


import io.ktor.application.*
import io.ktor.features.*
import io.ktor.http.*
import io.ktor.http.cio.websocket.*
import io.ktor.request.*
import io.ktor.response.*
import io.ktor.routing.*
import io.ktor.server.engine.*
import io.ktor.server.netty.*
import io.ktor.websocket.*
import kotlinx.coroutines.delay
import kotlinx.coroutines.runBlocking
import java.io.File
import java.nio.charset.StandardCharsets
import kotlin.concurrent.thread

class Room(val id:String) {

    val ADDR_SERVER = "serv"
    val ADDR_BROADCAST = "****"

    var users = mutableMapOf<String, WebSocketSession>()

    fun add(userId:String, socket:WebSocketSession){
        synchronized(users){
            users.put(userId, socket)
        }
    }

    suspend fun send(from:String, fromSocket: WebSocketSession, frame:Frame){
        val bytes: ByteArray = frame.readBytes()

        val addr = String(bytes.sliceArray(IntRange(0,3)), StandardCharsets.UTF_8)

        val content = bytes.sliceArray(IntRange(4, bytes.size-1))

        println("$id, send from: $from to: $addr")

        if (addr == ADDR_BROADCAST){
            val all = synchronized(users){users.filter { it.key != from }}
            all.forEach {
                send(fromSocket, it.key, it.value, content)
            }
        }
        else {
            while (true) {
                val user = synchronized(users) { users.get(addr) }
                if (user == null){
                    if (addr == ADDR_SERVER) {
                        delay(100)
                        continue
                    }
                    sendError(fromSocket, addr)
                    break;
                }

                send(fromSocket, addr, user, content)
                break
            }
        }
    }

    suspend fun sendError(socket: WebSocketSession, errorAddr:String){
        println("not found ${errorAddr}")
        val content = errorAddr.toByteArray(Charsets.UTF_8)

        try {
            socket.send(content)
        } catch (e:java.lang.Exception){
            println("err")
        }
    }

    suspend fun send(fromSocket: WebSocketSession, addr:String, toSocket: WebSocketSession, content:ByteArray){
        try {
            toSocket.send(content)
        } catch (e:java.lang.Exception){
            synchronized(users){
                println("$id, removed user: $addr")
                users.remove(addr)
            }
            sendError(fromSocket, addr)
        }
    }
}

class Ws {
    var waiting:WebSocketSession? = null
    var rooms = mutableMapOf<String, Room>()

    fun Routing.routing() {

        webSocket("/XsZubnMOTHC0JRDTS95S/{roomId}/{userId}") { // websocketSession

            val roomId = call.parameters["roomId"]!!
            val userId = call.parameters["userId"]!!

            val room = synchronized(rooms){
                rooms.getOrPut(roomId){
                    Room(roomId)
                }
            }

            room.add(userId, this)

            while (true) {
                for (frame in incoming)
                    room.send(userId, this, frame)

                delay(1)
            }

            /*
            rooms
            val room = rooms.getOrPut(roomId){
                return@getOrPut Room(roomId)
            }

            waiting = this;
            val me = this;


            while (waiting == me){
                delay(1)
            }

            val other = waiting!!;
            waiting = me;

           // val other = waiting!!;
            while (true) {
                for (frame in incoming)
                    other.send(frame)

                delay(1)
            }*/
        }
    }
}

fun main(args: Array<String>) {
    val ws = Ws()
    val env = applicationEngineEnvironment {

        module {
            install(WebSockets)
            install(CORS) {
                method(HttpMethod.Options)
                method(HttpMethod.Put)
                method(HttpMethod.Delete)
                method(HttpMethod.Patch)
                method(HttpMethod.Post)
                header("name")
                header(HttpHeaders.Authorization)
                allowCredentials = true
                anyHost()
            }

            routing {
                with(ws){
                    routing()
                }
                post("/upload"){

                    runBlocking {
                        val log = call.receiveText()
                        val name = call.request.header("name")!!

                        val time = System.currentTimeMillis()

                        val fileName = "${name}.txt";

                        val dir = File("logs");
                        if (!dir.exists())
                            dir.mkdirs();

                        File("logs/$fileName").writeText(log);
                        call.respondText("ok")
                    }
                }
            }
        }

        connector {
            host = "0.0.0.0"
            port = 9096
        }
    }

    embeddedServer(Netty, env).start()

    Runtime.getRuntime().addShutdownHook(thread(start = false) {
        println("shutting down app...")
    })
}

