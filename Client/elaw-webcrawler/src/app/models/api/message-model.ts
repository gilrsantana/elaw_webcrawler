import { MessageType } from "./message-type"

export type MessageModel = {
    type: MessageType;
    text: string;
}