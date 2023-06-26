import { ChatMessageVm } from './ChatMessageVm';

export interface GetChatResponseQuery {
  chatMessages?: ChatMessageVm[];
  chatConversationId?: number | undefined;
  currentUrl?: string;
  forceFunctionCall?: string;
}
