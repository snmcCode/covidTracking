package ca.snmc.scanner

import android.app.Application
import androidx.lifecycle.AndroidViewModel
import androidx.lifecycle.MutableLiveData
import ca.snmc.scanner.data.db.entities.SelectedEventEntity
import ca.snmc.scanner.data.providers.PreferenceProvider
import ca.snmc.scanner.data.repositories.BackEndRepository
import ca.snmc.scanner.data.repositories.DeviceIORepository
import ca.snmc.scanner.models.EventData

class MainViewModel(
    application: Application,
    private val backEndRepository: BackEndRepository,
    private val deviceIORepository: DeviceIORepository,
    private val prefs: PreferenceProvider
) : AndroidViewModel(application) {

    var eventData : MutableLiveData<EventData> = MutableLiveData(EventData())

    var visitLogFileLogCount = MutableLiveData<Int>(0)

    fun getScannerMode(): Int = prefs.readScannerMode()

    fun checkIfVisitLogFileExists() = deviceIORepository.checkIfFileExists()

    fun getVisitLogFileLogsCount() = deviceIORepository.getLogCountObservable()

    fun getSelectedEvent() = backEndRepository.getSelectedEvent()

    fun getEventById(eventId: Int) = backEndRepository.getEventById(eventId)

    fun getSelectedEventLiveAttendance(eventId: Int) = backEndRepository.getEventLiveAttendance(eventId)

    fun updateEventData(selectedEventEntity: SelectedEventEntity?) {
        if (selectedEventEntity != null) {
            if (selectedEventEntity.eventId != null) {
                val eventEntity = backEndRepository.getEventById(selectedEventEntity.eventId!!)
                eventData.postValue(
                    EventData(
                        eventId = eventEntity.id,
                        eventName = eventEntity.name,
                        eventCapacity = eventEntity.capacity,
                        eventAttendance = backEndRepository.getEventAttendance(eventId = eventEntity.id)
                    ))
            }
        } else {
            eventData.postValue(
                EventData(
                    eventId = null,
                    eventName = null,
                    eventCapacity = null,
                    eventAttendance = null
            ))
        }
    }

    fun updateEventAttendance() {
        if (eventData.value != null && eventData.value!!.eventId != null && eventData.value!!.eventName != null && eventData.value!!.eventCapacity != null) {
            eventData.postValue(
                EventData(
                    eventId = eventData.value!!.eventId,
                    eventName = eventData.value!!.eventName,
                    eventCapacity = eventData.value!!.eventCapacity,
                    eventAttendance = backEndRepository.getEventAttendance(eventId = eventData.value!!.eventId!!)
                ))
        }
    }

}