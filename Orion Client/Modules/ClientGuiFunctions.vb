﻿Imports System.Drawing
Imports System.Windows.Forms

Public Module ClientGuiFunctions
    Public Sub CheckGuiMove(ByVal X As Long, ByVal Y As Long)
        Dim eqNum As Long, InvNum As Long, skillslot As Long
        Dim bankitem As Long, shopslot As Long, TradeNum As Long

        If InMapEditor Then Exit Sub
        ShowItemDesc = False
        'Charpanel
        If pnlCharacterVisible Then
            If X > CharWindowX And X < CharWindowX + CharPanelGFXInfo.width Then
                If Y > CharWindowY And Y < CharWindowY + CharPanelGFXInfo.height Then
                    eqNum = IsEqItem(X, Y)
                    If eqNum <> 0 Then
                        UpdateDescWindow(GetPlayerEquipment(MyIndex, eqNum), 0, eqNum)
                        LastItemDesc = GetPlayerEquipment(MyIndex, eqNum) ' set it so you don't re-set values
                        ShowItemDesc = True
                        Exit Sub
                    Else
                        ShowItemDesc = False
                        LastItemDesc = 0 ' no item was last loaded
                    End If
                End If
            End If
        End If

        'inventory
        If pnlInventoryVisible Then
            If AboveInvpanel(X, Y) Then
                InvX = X
                InvY = Y

                If DragInvSlotNum > 0 Then
                    If InTrade Then Exit Sub
                    If InBank Or InShop Then Exit Sub
                    DrawInventoryItem(X, Y)
                    ShowItemDesc = False
                    LastItemDesc = 0 ' no item was last loaded
                Else
                    InvNum = IsInvItem(X, Y)

                    If InvNum <> 0 Then
                        ' exit out if we're offering that item
                        For i = 1 To MAX_INV
                            If TradeYourOffer(i).Num = InvNum Then
                                Exit Sub
                            End If
                        Next
                        UpdateDescWindow(GetPlayerInvItemNum(MyIndex, InvNum), GetPlayerInvItemValue(MyIndex, InvNum), InvNum)
                        LastItemDesc = GetPlayerInvItemNum(MyIndex, InvNum) ' set it so you don't re-set values
                        ShowItemDesc = True
                        Exit Sub
                    Else
                        ShowItemDesc = False
                        LastItemDesc = 0 ' no item was last loaded
                    End If
                End If
            End If
        End If

        'skills
        If pnlSkillsVisible = True Then
            If AboveSkillpanel(X, Y) Then
                SkillX = X
                SkillY = Y

                If DragSkillSlotNum > 0 Then
                    If InTrade Then Exit Sub
                    If InBank Or InShop Then Exit Sub
                    DrawSkillItem(X, Y)
                    LastSkillDesc = 0 ' no item was last loaded
                    ShowSkillDesc = False
                Else
                    skillslot = IsPlayerSkill(X, Y)

                    If skillslot <> 0 Then
                        UpdateSkillWindow(PlayerSkills(skillslot))
                        LastSkillDesc = PlayerSkills(skillslot)
                        ShowSkillDesc = True
                        Exit Sub
                    Else
                        LastSkillDesc = 0
                        ShowSkillDesc = False
                    End If
                End If

            End If
        End If

        'bank
        If pnlBankVisible = True Then
            If AboveBankpanel(X, Y) Then
                BankX = X
                BankY = Y

                If DragBankSlotNum > 0 Then
                    DrawBankItem(X, Y)
                Else
                    bankitem = IsBankItem(X, Y)

                    If bankitem <> 0 Then

                        UpdateDescWindow(Bank.Item(bankitem).Num, Bank.Item(bankitem).Value, bankitem)
                        ShowItemDesc = True
                        Exit Sub
                    Else
                        ShowItemDesc = False
                        LastItemDesc = 0 ' no item was last loaded
                    End If
                End If

            End If
        End If

        'shop
        If pnlShopVisible = True Then
            If AboveShoppanel(X, Y) Then
                shopslot = IsShopItem(X, Y)

                If shopslot <> 0 Then

                    UpdateDescWindow(Shop(InShop).TradeItem(shopslot).Item, Shop(InShop).TradeItem(shopslot).ItemValue, shopslot)
                    LastItemDesc = Shop(InShop).TradeItem(shopslot).Item
                    ShowItemDesc = True
                    Exit Sub
                Else
                    ShowItemDesc = False
                    LastItemDesc = 0
                End If

            End If
        End If

        'trade
        If pnlTradeVisible = True Then
            If AboveTradepanel(X, Y) Then
                TradeX = X
                TradeY = Y

                'ours
                TradeNum = IsTradeItem(X, Y, True)

                If TradeNum <> 0 Then
                    UpdateDescWindow(GetPlayerInvItemNum(MyIndex, TradeYourOffer(TradeNum).Num), TradeYourOffer(TradeNum).Value, TradeNum)
                    LastItemDesc = GetPlayerInvItemNum(MyIndex, TradeYourOffer(TradeNum).Num) ' set it so you don't re-set values
                    ShowItemDesc = True
                    Exit Sub
                Else
                    ShowItemDesc = False
                    LastItemDesc = 0
                End If

                'theirs
                TradeNum = IsTradeItem(X, Y, False)

                If TradeNum <> 0 Then
                    UpdateDescWindow(TradeTheirOffer(TradeNum).Num, TradeTheirOffer(TradeNum).Value, TradeNum)
                    LastItemDesc = TradeTheirOffer(TradeNum).Num ' set it so you don't re-set values
                    ShowItemDesc = True
                    Exit Sub
                Else
                    ShowItemDesc = False
                    LastItemDesc = 0
                End If
            End If
        End If



    End Sub

    Public Function CheckGuiClick(ByVal X As Long, ByVal Y As Long, ByVal e As MouseEventArgs) As Boolean
        Dim EqNum As Long, InvNum As Long
        Dim skillnum As Long, hotbarslot As Long
        Dim Buffer As ByteBuffer

        CheckGuiClick = False
        If InMapEditor Then Exit Function

        'action panel
        If HUDVisible Then
            If AboveActionPanel(X, Y) Then
                ' left click
                If e.Button = MouseButtons.Left Then
                    'Inventory
                    If X > ActionPanelX + InvBtnX And X < ActionPanelX + InvBtnX + 48 And Y > ActionPanelY + InvBtnY And Y < ActionPanelY + InvBtnY + 32 Then
                        PlaySound("Click.ogg")
                        pnlInventoryVisible = Not pnlInventoryVisible
                        pnlCharacterVisible = False
                        pnlSkillsVisible = False
                        frmMainGame.pnlOptions.Visible = False
                        CheckGuiClick = True
                        'Skills
                    ElseIf X > ActionPanelX + SkillBtnX And X < ActionPanelX + SkillBtnX + 48 And Y > ActionPanelY + SkillBtnY And Y < ActionPanelY + SkillBtnY + 32 Then
                        PlaySound("Click.ogg")
                        Buffer = New ByteBuffer
                        Buffer.WriteLong(ClientPackets.CSkills)
                        SendData(Buffer.ToArray())
                        Buffer = Nothing
                        pnlSkillsVisible = Not pnlSkillsVisible
                        pnlInventoryVisible = False
                        pnlCharacterVisible = False
                        frmMainGame.pnlOptions.Visible = False
                        CheckGuiClick = True
                        'Char
                    ElseIf X > ActionPanelX + CharBtnX And X < ActionPanelX + CharBtnX + 48 And Y > ActionPanelY + CharBtnY And Y < ActionPanelY + CharBtnY + 32 Then
                        PlaySound("Click.ogg")
                        SendRequestPlayerData()
                        pnlCharacterVisible = Not pnlCharacterVisible
                        pnlInventoryVisible = False
                        pnlSkillsVisible = False
                        frmMainGame.pnlOptions.Visible = False
                        CheckGuiClick = True
                        'Quest
                    ElseIf X > ActionPanelX + QuestBtnX And X < ActionPanelX + QuestBtnX + 48 And Y > ActionPanelY + QuestBtnY And Y < ActionPanelY + QuestBtnY + 32 Then
                        UpdateQuestLog()
                        ' show the window
                        pnlInventoryVisible = False
                        pnlCharacterVisible = False
                        frmMainGame.pnlOptions.Visible = False
                        RefreshQuestLog()
                        pnlQuestLogVisible = Not pnlQuestLogVisible
                        'frmMainGame.pnlQuestLog.BringToFront()
                        CheckGuiClick = True
                    ElseIf X > ActionPanelX + OptBtnX And X < ActionPanelX + OptBtnX + 48 And Y > ActionPanelY + OptBtnY And Y < ActionPanelY + OptBtnY + 32 Then
                        PlaySound("Click.ogg")
                        pnlCharacterVisible = False
                        pnlInventoryVisible = False
                        pnlSkillsVisible = False
                        frmMainGame.pnlOptions.BringToFront()
                        frmMainGame.pnlOptions.Visible = Not frmMainGame.pnlOptions.Visible
                        CheckGuiClick = True
                        'Exit
                    ElseIf X > ActionPanelX + ExitBtnX And X < ActionPanelX + ExitBtnX + 48 And Y > ActionPanelY + ExitBtnY And Y < ActionPanelY + ExitBtnY + 32 Then
                        PlaySound("Click.ogg")
                        frmAdmin.Dispose()
                        DestroyGame()
                        CheckGuiClick = True
                    End If
                End If
            End If

            'hotbar
            If AboveHotbar(X, Y) Then

                hotbarslot = IsHotBarSlot(e.Location.X, e.Location.Y)

                If e.Button = MouseButtons.Left Then
                    If hotbarslot > 0 Then
                        skillnum = PlayerSkills(Player(MyIndex).Hotbar(hotbarslot).Slot)

                        If skillnum <> 0 Then
                            PlaySound("Click.ogg")
                            PlayerCastSkill(skillnum)
                        End If
                    End If
                ElseIf e.Button = MouseButtons.Right Then ' right click
                    If Player(MyIndex).Hotbar(hotbarslot).Slot > 0 Then
                        'forget hotbar skill
                        SendDeleteHotbar(IsHotBarSlot(e.Location.X, e.Location.Y))
                        CheckGuiClick = True
                    Else
                        Buffer = New ByteBuffer
                        Buffer.WriteLong(ClientPackets.CSkills)
                        SendData(Buffer.ToArray())
                        Buffer = Nothing
                        pnlSkillsVisible = True
                        AddText("Click on the skill you want to place here", TellColor)
                        SelSkillSlot = True
                        SelHotbarSlot = IsHotBarSlot(e.Location.X, e.Location.Y)
                    End If
                End If
                CheckGuiClick = True
            End If
        End If

        'Charpanel
        If pnlCharacterVisible Then
            If AboveCharpanel(X, Y) Then
                ' left click
                If e.Button = MouseButtons.Left Then
                    'first check for equip
                    EqNum = IsEqItem(X, Y)

                    If EqNum <> 0 Then
                        PlaySound("Click.ogg")
                        SendUnequip(EqNum)
                    End If

                    'lets see if they want to upgrade
                    'Strenght
                    If X > CharWindowX + StrengthUpgradeX And X < CharWindowX + StrengthUpgradeX + 10 And Y > CharWindowY + StrengthUpgradeY And Y < CharWindowY + StrengthUpgradeY + 10 Then
                        If Not GetPlayerPOINTS(MyIndex) = 0 Then
                            PlaySound("Click.ogg")
                            SendTrainStat(1)
                        End If
                    End If
                    'Endurance
                    If X > CharWindowX + EnduranceUpgradeX And X < CharWindowX + EnduranceUpgradeX + 10 And Y > CharWindowY + EnduranceUpgradeY And Y < CharWindowY + EnduranceUpgradeY + 10 Then
                        If Not GetPlayerPOINTS(MyIndex) = 0 Then
                            PlaySound("Click.ogg")
                            SendTrainStat(2)
                        End If
                    End If
                    'Vitality
                    If X > CharWindowX + VitalityUpgradeX And X < CharWindowX + VitalityUpgradeX + 10 And Y > CharWindowY + VitalityUpgradeY And Y < CharWindowY + VitalityUpgradeY + 10 Then
                        If Not GetPlayerPOINTS(MyIndex) = 0 Then
                            PlaySound("Click.ogg")
                            SendTrainStat(3)
                        End If
                    End If
                    'WillPower
                    If X > CharWindowX + LuckUpgradeX And X < CharWindowX + LuckUpgradeX + 10 And Y > CharWindowY + LuckUpgradeY And Y < CharWindowY + LuckUpgradeY + 10 Then
                        If Not GetPlayerPOINTS(MyIndex) = 0 Then
                            PlaySound("Click.ogg")
                            SendTrainStat(4)
                        End If
                    End If
                    'Intellect
                    If X > CharWindowX + IntellectUpgradeX And X < CharWindowX + IntellectUpgradeX + 10 And Y > CharWindowY + IntellectUpgradeY And Y < CharWindowY + IntellectUpgradeY + 10 Then
                        If Not GetPlayerPOINTS(MyIndex) = 0 Then
                            PlaySound("Click.ogg")
                            SendTrainStat(5)
                        End If
                    End If
                    'Spirit
                    If X > CharWindowX + SpiritUpgradeX And X < CharWindowX + SpiritUpgradeX + 10 And Y > CharWindowY + SpiritUpgradeY And Y < CharWindowY + SpiritUpgradeY + 10 Then
                        If Not GetPlayerPOINTS(MyIndex) = 0 Then
                            PlaySound("Click.ogg")
                            SendTrainStat(6)
                        End If
                    End If
                    CheckGuiClick = True
                End If
            End If

            'Inventory panel
        ElseIf pnlInventoryVisible Then
            If AboveInvpanel(X, Y) Then
                InvNum = IsInvItem(e.Location.X, e.Location.Y)

                If e.Button = MouseButtons.Left Then
                    If InvNum <> 0 Then
                        If InTrade Then Exit Function
                        If InBank Or InShop Then Exit Function

                        If Item(GetPlayerInvItemNum(MyIndex, InvNum)).Type = ITEM_TYPE_FURNITURE Then
                            PlaySound("Click.ogg")
                            FurnitureSelected = InvNum
                            CheckGuiClick = True
                        End If

                    End If
                End If
            End If
        End If

        If DialogPanelVisible Then
            'ok button
            If X > DialogPanelX + OkButtonX And X < DialogPanelX + OkButtonX + ButtonGFXInfo.width And Y > DialogPanelY + OkButtonY And Y < DialogPanelY + OkButtonY + ButtonGFXInfo.height Then
                VbKeyDown = False
                VbKeyUp = False
                VbKeyLeft = False
                VbKeyRight = False

                If DialogType = DIALOGUE_TYPE_BUYHOME Then 'house offer
                    SendBuyHouse(1)
                ElseIf DialogType = DIALOGUE_TYPE_VISIT Then
                    SendVisit(1)
                ElseIf DialogType = DIALOGUE_TYPE_PARTY Then
                    SendJoinParty()
                ElseIf DialogType = DIALOGUE_TYPE_QUEST Then
                    If QuestAcceptTag > 0 Then
                        PlayerHandleQuest(QuestAcceptTag, 1)
                        QuestAcceptTag = 0
                        RefreshQuestLog()
                    End If
                ElseIf DialogType = DIALOGUE_TYPE_TRADE Then
                    SendTradeInviteAccept(1)
                End If

                PlaySound("Click.ogg")
                DialogPanelVisible = False
            End If
            'cancel button
            If X > DialogPanelX + CancelButtonX And X < DialogPanelX + CancelButtonX + ButtonGFXInfo.width And Y > DialogPanelY + CancelButtonY And Y < DialogPanelY + CancelButtonY + ButtonGFXInfo.height Then
                VbKeyDown = False
                VbKeyUp = False
                VbKeyLeft = False
                VbKeyRight = False

                If DialogType = DIALOGUE_TYPE_BUYHOME Then 'house offer declined
                    SendBuyHouse(0)
                ElseIf DIALOGUE_TYPE_VISIT Then 'visit declined
                    SendVisit(0)
                ElseIf DIALOGUE_TYPE_PARTY Then 'party declined
                    SendLeaveParty()
                ElseIf DIALOGUE_TYPE_QUEST Then 'quest declined
                    QuestAcceptTag = 0
                ElseIf DialogType = DIALOGUE_TYPE_TRADE Then
                    SendTradeInviteAccept(0)
                End If
                PlaySound("Click.ogg")
                DialogPanelVisible = False
            End If
            CheckGuiClick = True
        End If

        If pnlBankVisible = True Then
            If AboveBankpanel(X, Y) Then
                If X > BankWindowX + 140 And X < BankWindowX + 140 + getTextWidth("Close Bank", 15) Then
                    If Y > BankWindowY + BankPanelGFXInfo.height - 15 And Y < BankWindowY + BankPanelGFXInfo.height Then
                        PlaySound("Click.ogg")
                        CloseBank()
                    End If
                End If
                CheckGuiClick = True
            End If
        End If

        'trade
        If pnlTradeVisible = True Then
            If AboveTradepanel(X, Y) Then
                'accept button
                If X > TradeWindowX + TradeButtonAcceptX And X < TradeWindowX + TradeButtonAcceptX + ButtonGFXInfo.width Then
                    If Y > TradeWindowY + TradeButtonAcceptY And Y < TradeWindowY + TradeButtonAcceptY + ButtonGFXInfo.height Then
                        PlaySound("Click.ogg")
                        AcceptTrade()
                    End If
                End If

                'decline button
                If X > TradeWindowX + TradeButtonDeclineX And X < TradeWindowX + TradeButtonDeclineX + ButtonGFXInfo.width Then
                    If Y > TradeWindowY + TradeButtonDeclineY And Y < TradeWindowY + TradeButtonDeclineY + ButtonGFXInfo.height Then
                        PlaySound("Click.ogg")
                        DeclineTrade()
                    End If
                End If

                CheckGuiClick = True
            End If

        End If

        'eventchat
        If pnlEventChatVisible = True Then
            If AboveEventChat(X, Y) Then
                'Response1
                If EventChoiceVisible(1) Then
                    If X > EventChatX + 10 And X < EventChatX + 10 + getTextWidth(EventChoices(1)) Then
                        If Y > EventChatY + 124 And Y < EventChatY + 124 + 13 Then
                            PlaySound("Click.ogg")
                            Buffer = New ByteBuffer
                            Buffer.WriteLong(ClientPackets.CEventChatReply)
                            Buffer.WriteLong(EventReplyID)
                            Buffer.WriteLong(EventReplyPage)
                            Buffer.WriteLong(1)
                            SendData(Buffer.ToArray)
                            Buffer = Nothing
                            ClearEventChat()
                            InEvent = False
                        End If
                    End If
                End If

                'Response2
                If EventChoiceVisible(2) Then
                    If X > EventChatX + 10 And X < EventChatX + 10 + getTextWidth(EventChoices(2)) Then
                        If Y > EventChatY + 146 And Y < EventChatY + 146 + 13 Then
                            PlaySound("Click.ogg")
                            Buffer = New ByteBuffer
                            Buffer.WriteLong(ClientPackets.CEventChatReply)
                            Buffer.WriteLong(EventReplyID)
                            Buffer.WriteLong(EventReplyPage)
                            Buffer.WriteLong(2)
                            SendData(Buffer.ToArray)
                            Buffer = Nothing
                            ClearEventChat()
                            InEvent = False
                        End If
                    End If
                End If

                'Response3
                If EventChoiceVisible(3) Then
                    If X > EventChatX + 226 And X < EventChatX + 226 + getTextWidth(EventChoices(3)) Then
                        If Y > EventChatY + 124 And Y < EventChatY + 124 + 13 Then
                            PlaySound("Click.ogg")
                            Buffer = New ByteBuffer
                            Buffer.WriteLong(ClientPackets.CEventChatReply)
                            Buffer.WriteLong(EventReplyID)
                            Buffer.WriteLong(EventReplyPage)
                            Buffer.WriteLong(3)
                            SendData(Buffer.ToArray)
                            Buffer = Nothing
                            ClearEventChat()
                            InEvent = False
                        End If
                    End If
                End If

                'Response4
                If EventChoiceVisible(4) Then
                    If X > EventChatX + 226 And X < EventChatX + 226 + getTextWidth(EventChoices(4)) Then
                        If Y > EventChatY + 146 And Y < EventChatY + 146 + 13 Then
                            PlaySound("Click.ogg")
                            Buffer = New ByteBuffer
                            Buffer.WriteLong(ClientPackets.CEventChatReply)
                            Buffer.WriteLong(EventReplyID)
                            Buffer.WriteLong(EventReplyPage)
                            Buffer.WriteLong(4)
                            SendData(Buffer.ToArray)
                            Buffer = Nothing
                            ClearEventChat()
                            InEvent = False
                        End If
                    End If
                End If

                'continue
                If EventChatType <> 1 Then
                    If X > EventChatX + 410 And X < EventChatX + 410 + getTextWidth("Continue") Then
                        If Y > EventChatY + 156 And Y < EventChatY + 156 + 13 Then
                            PlaySound("Click.ogg")
                            Buffer = New ByteBuffer
                            Buffer.WriteLong(ClientPackets.CEventChatReply)
                            Buffer.WriteLong(EventReplyID)
                            Buffer.WriteLong(EventReplyPage)
                            Buffer.WriteLong(0)
                            SendData(Buffer.ToArray)
                            Buffer = Nothing
                            ClearEventChat()
                            InEvent = False
                        End If
                    End If
                End If
                CheckGuiClick = True
            End If
        End If

        'right click
        If pnlRClickVisible = True Then
            If AboveRClickPanel(X, Y) Then
                'trade
                If X > RClickX + (RClickGFXInfo.width \ 2) - (getTextWidth("Invite to Trade") \ 2) And X < RClickX + (RClickGFXInfo.width \ 2) - (getTextWidth("Invite to Trade") \ 2) + getTextWidth("Invite to Trade") Then
                    If Y > RClickY + 35 And Y < RClickY + 35 + 12 Then
                        If myTarget > 0 Then
                            SendTradeRequest(Player(myTarget).Name)
                        End If
                        pnlRClickVisible = False
                    End If
                End If

                'party
                If X > RClickX + (RClickGFXInfo.width \ 2) - (getTextWidth("Invite to Party") \ 2) And X < RClickX + (RClickGFXInfo.width \ 2) - (getTextWidth("Invite to Party") \ 2) + getTextWidth("Invite to Party") Then
                    If Y > RClickY + 60 And Y < RClickY + 60 + 12 Then
                        If myTarget > 0 Then
                            SendPartyRequest(Player(myTarget).Name)
                        End If
                        pnlRClickVisible = False
                    End If
                End If

                'House
                If X > RClickX + (RClickGFXInfo.width \ 2) - (getTextWidth("Invite to House") \ 2) And X < RClickX + (RClickGFXInfo.width \ 2) - (getTextWidth("Invite to House") \ 2) + getTextWidth("Invite to House") Then
                    If Y > RClickY + 85 And Y < RClickY + 85 + 12 Then
                        If myTarget > 0 Then
                            SendInvite(Player(myTarget).Name)
                        End If
                        pnlRClickVisible = False
                    End If
                End If

                CheckGuiClick = True
            End If
        End If

        If pnlQuestLogVisible Then
            If AboveQuestPanel(X, Y) Then
                'check if they press the list
                Dim tmpy As Long = 10
                For i = 1 To MAX_ACTIVEQUESTS
                    If Len(Trim$(QuestNames(i))) > 0 Then
                        If X > (QuestLogX + 7) And X < (QuestLogX + 7) + (getTextWidth(QuestNames(i))) Then
                            If Y > (QuestLogY + tmpy) And Y < (QuestLogY + tmpy + 13) Then
                                SelectedQuest = i
                                LoadQuestlogBox()
                            End If
                        End If
                        tmpy = tmpy + 20
                    End If
                Next

                'close button
                If X > (QuestLogX + 195) And X < (QuestLogX + 290) Then
                    If Y > (QuestLogY + 358) And Y < (QuestLogY + 375) Then
                        ResetQuestLog()
                    End If

                    CheckGuiClick = True
                End If
            End If
        End If

        If pnlCraftVisible Then
            If AboveCraftPanel(X, Y) Then
                'check if they press the list
                Dim tmpy As Long = 10
                For i = 1 To MAX_RECIPE
                    If Len(Trim$(RecipeNames(i))) > 0 Then
                        If X > (CraftPanelX + 12) And X < (CraftPanelX + 12) + (getTextWidth(RecipeNames(i))) Then
                            If Y > (CraftPanelY + tmpy) And Y < (CraftPanelY + tmpy + 13) Then
                                SelectedRecipe = i
                                CraftingInit()
                            End If
                        End If
                        tmpy = tmpy + 20
                    End If
                Next

                'start button
                If X > (CraftPanelX + 256) And X < (CraftPanelX + 330) Then
                    If Y > (CraftPanelY + 415) And Y < (CraftPanelY + 437) Then
                        If SelectedRecipe > 0 Then
                            SendCraftIt(RecipeNames(SelectedRecipe), CraftAmountValue)
                        End If
                    End If
                End If

                'close button
                If X > (CraftPanelX + 614) And X < (CraftPanelX + 689) Then
                    If Y > (CraftPanelY + 472) And Y < (CraftPanelY + 494) Then
                        ResetCraftPanel()
                        pnlCraftVisible = False
                        InCraft = False
                        SendCloseCraft()
                    End If
                End If

                'minus
                If X > (CraftPanelX + 340) And X < (CraftPanelX + 340 + 10) Then
                    If Y > (CraftPanelY + 422) And Y < (CraftPanelY + 422 + 10) Then
                        If CraftAmountValue > 1 Then
                            CraftAmountValue = CraftAmountValue - 1
                        End If
                    End If
                End If

                'plus
                If X > (CraftPanelX + 392) And X < (CraftPanelX + 392 + 10) Then
                    If Y > (CraftPanelY + 422) And Y < (CraftPanelY + 422 + 10) Then
                        If CraftAmountValue < 100 Then
                            CraftAmountValue = CraftAmountValue + 1
                        End If
                    End If
                End If

                CheckGuiClick = True
            End If
        End If

    End Function

    Public Function CheckGuiDoubleClick(ByVal X As Long, ByVal Y As Long, ByVal e As MouseEventArgs) As Boolean
        Dim InvNum As Long, skillnum As Long, BankItem As Long
        Dim Value As Long, TradeNum As Long
        Dim multiplier As Double
        Dim i As Long

        If pnlInventoryVisible Then
            If AboveInvpanel(X, Y) Then
                DragInvSlotNum = 0
                InvNum = IsInvItem(InvX, InvY)

                If InvNum <> 0 Then

                    ' are we in a shop?
                    If InShop > 0 Then
                        Select Case ShopAction
                            Case 0 ' nothing, give value
                                multiplier = Shop(InShop).BuyRate / 100
                                Value = Item(GetPlayerInvItemNum(MyIndex, InvNum)).Price * multiplier
                                If Value > 0 Then
                                    AddText("You can sell this item for " & Value & " gold.", TellColor)
                                Else
                                    AddText("The shop does not want this item.", AlertColor)
                                End If
                            Case 2 ' 2 = sell
                                SellItem(InvNum)
                        End Select

                        Exit Function
                    End If

                    ' in bank?
                    If InBank Then
                        If Item(GetPlayerInvItemNum(MyIndex, InvNum)).Type = ITEM_TYPE_CURRENCY Or Item(GetPlayerInvItemNum(MyIndex, InvNum)).Stackable = 1 Then
                            CurrencyMenu = 2 ' deposit
                            frmMainGame.lblCurrency.Text = "How many do you want to deposit?"
                            tmpCurrencyItem = InvNum
                            frmMainGame.txtCurrency.Text = vbNullString
                            frmMainGame.pnlCurrency.Visible = True
                            frmMainGame.pnlCurrency.BringToFront()
                            frmMainGame.txtCurrency.Focus()
                            Exit Function
                        End If
                        DepositItem(InvNum, 0)
                        Exit Function
                    End If

                    ' in trade?
                    If InTrade = True Then
                        ' exit out if we're offering that item
                        For i = 1 To MAX_INV
                            If TradeYourOffer(i).Num = InvNum Then
                                Exit Function
                            End If
                        Next
                        If Item(GetPlayerInvItemNum(MyIndex, InvNum)).Type = ITEM_TYPE_CURRENCY Or Item(GetPlayerInvItemNum(MyIndex, InvNum)).Stackable = 1 Then
                            ' currency shit here brah
                            Exit Function
                        End If
                        TradeItem(InvNum, 0)
                        Exit Function
                    End If

                    ' use item if not doing anything else
                    If Item(GetPlayerInvItemNum(MyIndex, InvNum)).Type = ITEM_TYPE_NONE Then Exit Function
                    SendUseItem(InvNum)
                    Exit Function
                End If
            End If
        End If

        'Skill panel
        If pnlSkillsVisible = True Then
            If AboveSkillpanel(X, Y) Then

                skillnum = IsPlayerSkill(SkillX, SkillY)

                If skillnum <> 0 Then
                    PlayerCastSkill(skillnum)
                    Exit Function
                End If
            End If
        End If

        'Bank panel
        If pnlBankVisible = True Then
            If AboveBankpanel(X, Y) Then

                DragBankSlotNum = 0

                BankItem = IsBankItem(BankX, BankY)
                If BankItem <> 0 Then
                    If GetBankItemNum(BankItem) = ITEM_TYPE_NONE Then Exit Function

                    If Item(GetBankItemNum(BankItem)).Type = ITEM_TYPE_CURRENCY Or Item(GetBankItemNum(BankItem)).Stackable = 1 Then
                        CurrencyMenu = 3 ' withdraw
                        frmMainGame.lblCurrency.Text = "How many do you want to withdraw?"
                        tmpCurrencyItem = BankItem
                        frmMainGame.txtCurrency.Text = vbNullString
                        frmMainGame.pnlCurrency.Visible = True
                        frmMainGame.txtCurrency.Focus()
                        Exit Function
                    End If

                    WithdrawItem(BankItem, 0)
                    Exit Function
                End If
            End If
        End If

        'trade panel
        If pnlTradeVisible = True Then
            'ours?
            If AboveTradepanel(X, Y) Then
                TradeNum = IsTradeItem(TradeX, TradeY, True)

                If TradeNum <> 0 Then
                    UntradeItem(TradeNum)
                End If
            End If
        End If

    End Function

    Public Function CheckGuiMouseUp(ByVal X As Long, ByVal Y As Long, ByVal e As MouseEventArgs) As Boolean
        Dim i As Long, rec_pos As Rectangle, buffer As ByteBuffer
        'Inventory
        If pnlInventoryVisible Then
            If AboveInvpanel(X, Y) Then
                If InTrade > 0 Then Exit Function
                If InBank Or InShop Then Exit Function

                If DragInvSlotNum > 0 Then

                    For i = 1 To MAX_INV

                        With rec_pos
                            .Y = InvWindowY + InvTop + ((InvOffsetY + 32) * ((i - 1) \ InvColumns))
                            .Height = PIC_Y
                            .X = InvWindowX + InvLeft + ((InvOffsetX + 32) * (((i - 1) Mod InvColumns)))
                            .Width = PIC_X
                        End With

                        If e.Location.X >= rec_pos.Left And e.Location.X <= rec_pos.Right Then
                            If e.Location.Y >= rec_pos.Top And e.Location.Y <= rec_pos.Bottom Then '
                                If DragInvSlotNum <> i Then
                                    SendChangeInvSlots(DragInvSlotNum, i)
                                    Exit For
                                End If
                            End If
                        End If

                    Next

                End If

                DragInvSlotNum = 0
                frmMainGame.pnlTmpInv.Visible = False
            Else
                If FurnitureSelected > 0 Then
                    If Player(MyIndex).InHouse = MyIndex Then
                        If Item(PlayerInv(FurnitureSelected).Num).Type = ITEM_TYPE_FURNITURE Then
                            buffer = New ByteBuffer
                            buffer.WriteLong(ClientPackets.CPlaceFurniture)
                            i = CurX
                            buffer.WriteLong(i)
                            i = CurY
                            buffer.WriteLong(i)
                            buffer.WriteLong(FurnitureSelected)
                            SendData(buffer.ToArray)
                            buffer = Nothing

                            FurnitureSelected = 0
                        End If
                    End If
                End If
            End If
        End If

        'skills
        If pnlSkillsVisible Then
            If AboveSkillpanel(X, Y) Then
                If InTrade > 0 Then Exit Function
                If InBank Or InShop Then Exit Function

                If DragSkillSlotNum > 0 Then

                    For i = 1 To MAX_PLAYER_SKILLS

                        With rec_pos
                            .Y = SkillWindowY + SkillTop + ((SkillOffsetY + 32) * ((i - 1) \ SkillColumns))
                            .Height = PIC_Y
                            .X = SkillWindowX + SkillLeft + ((SkillOffsetX + 32) * (((i - 1) Mod SkillColumns)))
                            .Width = PIC_X
                        End With

                        If e.Location.X >= rec_pos.Left And e.Location.X <= rec_pos.Right Then
                            If e.Location.Y >= rec_pos.Top And e.Location.Y <= rec_pos.Bottom Then '
                                If DragSkillSlotNum <> i Then
                                    'SendChangeSkillSlots(DragSkillSlotNum, i)
                                    Exit For
                                End If
                            End If
                        End If

                    Next

                End If

                DragSkillSlotNum = 0
                frmMainGame.pnlTmpSkill.Visible = False
            End If
        End If

        'bank
        If pnlBankVisible = True Then
            If AboveBankpanel(X, Y) Then
                ' TODO : Add sub to change bankslots client side first so there's no delay in switching
                If DragBankSlotNum > 0 Then
                    For i = 1 To MAX_BANK
                        With rec_pos
                            .Y = BankWindowY + BankTop + ((BankOffsetY + 32) * ((i - 1) \ BankColumns))
                            .Height = PIC_Y
                            .X = BankWindowX + BankLeft + ((BankOffsetX + 32) * (((i - 1) Mod BankColumns)))
                            .Width = PIC_X
                        End With

                        If X >= rec_pos.Left And X <= rec_pos.Right Then
                            If Y >= rec_pos.Top And Y <= rec_pos.Bottom Then
                                If DragBankSlotNum <> i Then
                                    ChangeBankSlots(DragBankSlotNum, i)
                                    Exit For
                                End If
                            End If
                        End If
                    Next
                End If

                DragBankSlotNum = 0
                frmMainGame.pnlTempBank.Visible = False
            End If
        End If

    End Function

    Public Function CheckGuiMouseDown(ByVal X As Long, ByVal Y As Long, ByVal e As MouseEventArgs) As Boolean
        Dim InvNum As Long, skillnum As Long, bankNum As Long, shopItem As Long

        'Inventory
        If pnlInventoryVisible Then
            If AboveInvpanel(X, Y) Then
                InvNum = IsInvItem(e.Location.X, e.Location.Y)

                If e.Button = MouseButtons.Left Then
                    If InvNum <> 0 Then
                        If InTrade Then Exit Function
                        If InBank Or InShop Then Exit Function
                        DragInvSlotNum = InvNum
                    End If
                ElseIf e.Button = MouseButtons.Right Then
                    If Not InBank And Not InShop And Not InTrade Then
                        If InvNum <> 0 Then
                            If Item(GetPlayerInvItemNum(MyIndex, InvNum)).Type = ITEM_TYPE_CURRENCY Or Item(GetPlayerInvItemNum(MyIndex, InvNum)).Stackable = 1 Then
                                If GetPlayerInvItemValue(MyIndex, InvNum) > 0 Then
                                    CurrencyMenu = 1 ' drop
                                    frmMainGame.lblCurrency.Text = "How many do you want to drop?"
                                    tmpCurrencyItem = InvNum
                                    frmMainGame.txtCurrency.Text = vbNullString
                                    frmMainGame.pnlCurrency.Visible = True
                                    frmMainGame.txtCurrency.Focus()
                                End If
                            Else
                                SendDropItem(InvNum, 0)
                            End If
                        End If
                    End If
                End If
            End If
        End If

        'skills
        If pnlSkillsVisible = True Then
            If AboveSkillpanel(X, Y) Then
                skillnum = IsPlayerSkill(e.Location.X, e.Location.Y)

                If e.Button = MouseButtons.Left Then
                    If skillnum <> 0 Then
                        If InTrade Then Exit Function

                        DragSkillSlotNum = skillnum

                        If SelSkillSlot = True Then
                            SendSetHotbarSkill(SelHotbarSlot, skillnum)
                        End If
                    End If
                ElseIf e.Button = MouseButtons.Right Then ' right click

                    If skillnum <> 0 Then
                        ForgetSkill(skillnum)
                        Exit Function
                    End If
                End If
            End If
        End If

        'Bank
        If pnlBankVisible = True Then
            If AboveBankpanel(X, Y) Then
                bankNum = IsBankItem(X, Y)

                If bankNum <> 0 Then

                    If e.Button = MouseButtons.Left Then
                        DragBankSlotNum = bankNum
                    End If

                End If
            End If
        End If

        'Shop
        If pnlShopVisible = True Then
            If AboveShoppanel(X, Y) Then
                shopItem = IsShopItem(X, Y)

                If shopItem > 0 Then
                    Select Case ShopAction
                        Case 0 ' no action, give cost
                            With Shop(InShop).TradeItem(shopItem)
                                AddText("You can buy this item for " & .CostValue & " " & Trim$(Item(.CostItem).Name) & ".", Yellow)
                            End With
                        Case 1 ' buy item
                            ' buy item code
                            BuyItem(shopItem)
                    End Select
                Else
                    ' check for buy button
                    If X > ShopWindowX + ShopButtonBuyX And X < ShopWindowX + ShopButtonBuyX + ButtonGFXInfo.width Then
                        If Y > ShopWindowY + ShopButtonBuyY And Y < ShopWindowY + ShopButtonBuyY + ButtonGFXInfo.height Then
                            If ShopAction = 1 Then Exit Function
                            ShopAction = 1 ' buying an item
                            AddText("Click on the item in the shop you wish to buy.", Yellow)
                        End If
                    End If
                    ' check for sell button
                    If X > ShopWindowX + ShopButtonSellX And X < ShopWindowX + ShopButtonSellX + ButtonGFXInfo.width Then
                        If Y > ShopWindowY + ShopButtonSellY And Y < ShopWindowY + ShopButtonSellY + ButtonGFXInfo.height Then
                            If ShopAction = 2 Then Exit Function
                            ShopAction = 2 ' selling an item
                            AddText("Double-click on the item in your inventory you wish to sell.", Yellow)
                        End If
                    End If
                    ' check for close button
                    If X > ShopWindowX + ShopButtonCloseX And X < ShopWindowX + ShopButtonCloseX + ButtonGFXInfo.width Then
                        If Y > ShopWindowY + ShopButtonCloseY And Y < ShopWindowY + ShopButtonCloseY + ButtonGFXInfo.height Then
                            Dim Buffer As ByteBuffer
                            Buffer = New ByteBuffer
                            Buffer.WriteLong(ClientPackets.CCloseShop)
                            SendData(Buffer.ToArray())
                            Buffer = Nothing
                            pnlShopVisible = False
                            InShop = 0
                            ShopAction = 0
                        End If
                    End If
                End If
            End If
        End If

        If HUDVisible = True Then
            If AboveChatScrollUp(X, Y) Then
                If ScrollMod + FirstLineIndex < MaxChatDisplayLines Then
                    ScrollMod = ScrollMod + 1
                End If
            End If
            If AboveChatScrollDown(X, Y) Then
                If ScrollMod - 1 >= 0 Then
                    ScrollMod = ScrollMod - 1
                End If
            End If
        End If

    End Function

#Region "Support Functions"
    Function IsEqItem(ByVal X As Single, ByVal Y As Single) As Long
        Dim tempRec As RECT
        Dim i As Long
        IsEqItem = 0

        For i = 1 To Equipment.Equipment_Count - 1

            If GetPlayerEquipment(MyIndex, i) > 0 And GetPlayerEquipment(MyIndex, i) <= MAX_ITEMS Then

                With tempRec
                    .top = CharWindowY + EqTop + ((EqOffsetY + 32) * ((i - 1) \ EqColumns))
                    .bottom = .top + PIC_Y
                    .left = CharWindowX + EqLeft + ((EqOffsetX + 32) * (((i - 1) Mod EqColumns)))
                    .right = .left + PIC_X
                End With

                If X >= tempRec.left And X <= tempRec.right Then
                    If Y >= tempRec.top And Y <= tempRec.bottom Then
                        IsEqItem = i
                        Exit Function
                    End If
                End If
            End If

        Next

    End Function

    Function IsInvItem(ByVal X As Single, ByVal Y As Single) As Long
        Dim tempRec As RECT
        Dim i As Long
        IsInvItem = 0

        For i = 1 To MAX_INV

            If GetPlayerInvItemNum(MyIndex, i) > 0 And GetPlayerInvItemNum(MyIndex, i) <= MAX_ITEMS Then

                With tempRec
                    .top = InvWindowY + InvTop + ((InvOffsetY + 32) * ((i - 1) \ InvColumns))
                    .bottom = .top + PIC_Y
                    .left = InvWindowX + InvLeft + ((InvOffsetX + 32) * (((i - 1) Mod InvColumns)))
                    .right = .left + PIC_X
                End With

                If X >= tempRec.left And X <= tempRec.right Then
                    If Y >= tempRec.top And Y <= tempRec.bottom Then
                        IsInvItem = i
                        Exit Function
                    End If
                End If
            End If

        Next

    End Function

    Function IsPlayerSkill(ByVal X As Single, ByVal Y As Single) As Long
        Dim tempRec As RECT
        Dim i As Long

        IsPlayerSkill = 0

        For i = 1 To MAX_PLAYER_SKILLS

            If PlayerSkills(i) > 0 And PlayerSkills(i) <= MAX_PLAYER_SKILLS Then

                With tempRec
                    .top = SkillWindowY + SkillTop + ((SkillOffsetY + 32) * ((i - 1) \ SkillColumns))
                    .bottom = .top + PIC_Y
                    .left = SkillWindowX + SkillLeft + ((SkillOffsetX + 32) * (((i - 1) Mod SkillColumns)))
                    .right = .left + PIC_X
                End With

                If X >= tempRec.left And X <= tempRec.right Then
                    If Y >= tempRec.top And Y <= tempRec.bottom Then
                        IsPlayerSkill = i
                        Exit Function
                    End If
                End If
            End If

        Next

    End Function

    Function IsBankItem(ByVal X As Single, ByVal Y As Single) As Long
        Dim tempRec As RECT
        Dim i As Long

        IsBankItem = 0

        For i = 1 To MAX_BANK
            If GetBankItemNum(i) > 0 And GetBankItemNum(i) <= MAX_ITEMS Then

                With tempRec
                    .top = BankWindowY + BankTop + ((BankOffsetY + 32) * ((i - 1) \ BankColumns))
                    .bottom = .top + PIC_Y
                    .left = BankWindowX + BankLeft + ((BankOffsetX + 32) * (((i - 1) Mod BankColumns)))
                    .right = .left + PIC_X
                End With

                If X >= tempRec.left And X <= tempRec.right Then
                    If Y >= tempRec.top And Y <= tempRec.bottom Then

                        IsBankItem = i
                        Exit Function
                    End If
                End If
            End If
        Next
    End Function

    Function IsShopItem(ByVal X As Single, ByVal Y As Single) As Long
        Dim tempRec As Rectangle
        Dim i As Long
        IsShopItem = 0

        For i = 1 To MAX_TRADES

            If Shop(InShop).TradeItem(i).Item > 0 And Shop(InShop).TradeItem(i).Item <= MAX_ITEMS Then
                With tempRec
                    .Y = ShopWindowY + ShopTop + ((ShopOffsetY + 32) * ((i - 1) \ ShopColumns))
                    .Height = PIC_Y
                    .X = ShopWindowX + ShopLeft + ((ShopOffsetX + 32) * (((i - 1) Mod ShopColumns)))
                    .Width = PIC_X
                End With

                If X >= tempRec.Left And X <= tempRec.Right Then
                    If Y >= tempRec.Top And Y <= tempRec.Bottom Then
                        IsShopItem = i
                        Exit Function
                    End If
                End If
            End If
        Next
    End Function

    Function IsTradeItem(ByVal X As Single, ByVal Y As Single, ByVal Yours As Boolean) As Long
        Dim tempRec As RECT
        Dim i As Long
        Dim itemnum As Long

        IsTradeItem = 0

        For i = 1 To MAX_INV

            If Yours Then
                itemnum = GetPlayerInvItemNum(MyIndex, TradeYourOffer(i).Num)

                With tempRec
                    .top = TradeWindowY + OurTradeY + InvTop + ((InvOffsetY + 32) * ((i - 1) \ InvColumns))
                    .bottom = .top + PIC_Y
                    .left = TradeWindowX + OurTradeX + InvLeft + ((InvOffsetX + 32) * (((i - 1) Mod InvColumns)))
                    .right = .left + PIC_X
                End With
            Else
                itemnum = TradeTheirOffer(i).Num

                With tempRec
                    .top = TradeWindowY + TheirTradeY + InvTop + ((InvOffsetY + 32) * ((i - 1) \ InvColumns))
                    .bottom = .top + PIC_Y
                    .left = TradeWindowX + TheirTradeX + InvLeft + ((InvOffsetX + 32) * (((i - 1) Mod InvColumns)))
                    .right = .left + PIC_X
                End With
            End If

            If itemnum > 0 And itemnum <= MAX_ITEMS Then

                If X >= tempRec.left And X <= tempRec.right Then
                    If Y >= tempRec.top And Y <= tempRec.bottom Then
                        IsTradeItem = i
                        Exit Function
                    End If
                End If

            End If

        Next

    End Function

    Function AboveActionPanel(ByVal X As Single, ByVal Y As Single) As Boolean
        AboveActionPanel = False

        If X > ActionPanelX And X < ActionPanelX + ActionPanelGFXInfo.width Then
            If Y > ActionPanelY And Y < ActionPanelY + ActionPanelGFXInfo.height Then
                AboveActionPanel = True
            End If
        End If
    End Function

    Function AboveHotbar(ByVal X As Single, ByVal Y As Single) As Boolean
        AboveHotbar = False

        If X > HotbarX And X < HotbarX + HotBarGFXInfo.width Then
            If Y > HotbarY And Y < HotbarY + HotBarGFXInfo.height Then
                AboveHotbar = True
            End If
        End If
    End Function

    Function AboveInvpanel(ByVal X As Single, ByVal Y As Single) As Boolean
        AboveInvpanel = False

        If X > InvWindowX And X < InvWindowX + InvPanelGFXInfo.width Then
            If Y > InvWindowY And Y < InvWindowY + InvPanelGFXInfo.height Then
                AboveInvpanel = True
            End If
        End If
    End Function

    Function AboveCharpanel(ByVal X As Single, ByVal Y As Single) As Boolean
        AboveCharpanel = False

        If X > CharWindowX And X < CharWindowX + CharPanelGFXInfo.width Then
            If Y > CharWindowY And Y < CharWindowY + CharPanelGFXInfo.height Then
                AboveCharpanel = True
            End If
        End If
    End Function

    Function AboveSkillpanel(ByVal X As Single, ByVal Y As Single) As Boolean
        AboveSkillpanel = False

        If X > SkillWindowX And X < SkillWindowX + SkillPanelGFXInfo.width Then
            If Y > SkillWindowY And Y < SkillWindowY + SkillPanelGFXInfo.height Then
                AboveSkillpanel = True
            End If
        End If
    End Function

    Function AboveBankpanel(ByVal X As Single, ByVal Y As Single) As Boolean
        AboveBankpanel = False

        If X > BankWindowX And X < BankWindowX + BankPanelGFXInfo.width Then
            If Y > BankWindowY And Y < BankWindowY + BankPanelGFXInfo.height Then
                AboveBankpanel = True
            End If
        End If
    End Function

    Function AboveShoppanel(ByVal X As Single, ByVal Y As Single) As Boolean
        AboveShoppanel = False

        If X > ShopWindowX And X < ShopWindowX + ShopPanelGFXInfo.width Then
            If Y > ShopWindowY And Y < ShopWindowY + ShopPanelGFXInfo.height Then
                AboveShoppanel = True
            End If
        End If
    End Function

    Function AboveTradepanel(ByVal X As Single, ByVal Y As Single) As Boolean
        AboveTradepanel = False

        If X > TradeWindowX And X < TradeWindowX + TradePanelGFXInfo.width Then
            If Y > TradeWindowY And Y < TradeWindowY + TradePanelGFXInfo.height Then
                AboveTradepanel = True
            End If
        End If
    End Function

    Function AboveEventChat(ByVal X As Single, ByVal Y As Single) As Boolean
        AboveEventChat = False

        If X > EventChatX And X < EventChatX + EventChatGFXInfo.width Then
            If Y > EventChatY And Y < EventChatY + EventChatGFXInfo.height Then
                AboveEventChat = True
            End If
        End If
    End Function

    Function AboveChatScrollUp(ByVal X As Single, ByVal Y As Single) As Boolean
        AboveChatScrollUp = False

        If X > ChatWindowX + ChatWindowGFXInfo.width - 24 And X < ChatWindowX + ChatWindowGFXInfo.width Then
            If Y > ChatWindowY And Y < ChatWindowY + 24 Then 'ChatWindowGFXInfo.height Then
                AboveChatScrollUp = True
            End If
        End If
    End Function

    Function AboveChatScrollDown(ByVal X As Single, ByVal Y As Single) As Boolean
        AboveChatScrollDown = False

        If X > ChatWindowX + ChatWindowGFXInfo.width - 24 And X < ChatWindowX + ChatWindowGFXInfo.width Then
            If Y > ChatWindowY + ChatWindowGFXInfo.height - 24 And Y < ChatWindowY + ChatWindowGFXInfo.height Then
                AboveChatScrollDown = True
            End If
        End If
    End Function

    Function AboveRClickPanel(ByVal X As Single, ByVal Y As Single) As Boolean
        AboveRClickPanel = False

        If X > RClickX And X < RClickX + RClickGFXInfo.width Then
            If Y > RClickY And Y < RClickY + RClickGFXInfo.height Then
                AboveRClickPanel = True
            End If
        End If
    End Function

    Function AboveQuestPanel(ByVal X As Single, ByVal Y As Single) As Boolean
        AboveQuestPanel = False

        If X > QuestLogX And X < QuestLogX + QuestGFXInfo.width Then
            If Y > QuestLogY And Y < QuestLogY + QuestGFXInfo.height Then
                AboveQuestPanel = True
            End If
        End If
    End Function

    Function AboveCraftPanel(ByVal X As Single, ByVal Y As Single) As Boolean
        AboveCraftPanel = False

        If X > CraftPanelX And X < CraftPanelX + CraftGFXInfo.width Then
            If Y > CraftPanelY And Y < CraftPanelY + CraftGFXInfo.height Then
                AboveCraftPanel = True
            End If
        End If
    End Function
#End Region


End Module
