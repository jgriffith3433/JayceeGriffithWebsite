
export interface ChatMessageVm {
  from?: string;
  name?: string;
  content?: string;
  rawContent?: string;
  functionCall?: string;
  forceFunctionCall?: string;
  received: boolean;
  to?: string;
}
