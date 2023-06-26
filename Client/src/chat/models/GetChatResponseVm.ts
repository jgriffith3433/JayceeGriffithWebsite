import { ChatMessageVm } from './ChatMessageVm';

export interface GetChatResponseVm {
  chatConversationId: number;
  createNewChat?: boolean;
  error?: boolean;
  dirty?: boolean;
  navigateToPage?: string;
  chatMessages?: ChatMessageVm[];
  forceFunctionCall: string | undefined;
}
